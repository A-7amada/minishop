using System.Text.RegularExpressions;
using MiniShop.Domain.Interfaces;
namespace MiniShop.Application.Services;
// Deterministic, read-only knowledge assistant. No external network calls or LLM.
public class AssistantService(IProductRepository products,IOrderRepository orders,IShopRepository shop)
{
 public async Task<string> AnswerAsync(string? question,bool ar,string? email)
 {
  var q=(question??"").Trim().ToLowerInvariant();
  string T(string en,string arabic)=>ar?arabic:en;
  bool Has(params string[] words)=>words.Any(q.Contains);
  if(q.Length<2 || q.Length>500)return T("Ask a question of 2 to 500 characters.","اكتب سؤالاً من حرفين إلى ٥٠٠ حرف.");
  if(Has("order","track","طلب","شحنه","تتبع","اوردر"))
  {
   if(email is null)return T("Sign in, then ask about your orders. I never show another customer's orders.","سجّل الدخول ثم اسأل عن طلباتك. لا أعرض طلبات أي عميل آخر.");
   var list=await orders.ListByUserAsync(email);
   var digits=Regex.Match(q,@"\d+");
   if(digits.Success)list=list.Where(o=>o.Id.ToString()==digits.Value).ToList();
   if(list.Count==0)return T("No matching orders in your account. Open My orders to check.","لا توجد طلبات مطابقة في حسابك. راجع صفحة طلباتي.");
   return string.Join("\n",list.Take(3).Select(o=>$"#{o.Id}: "+T(o.Status,o.Status switch{"Confirmed"=>"تم التأكيد","Processing"=>"قيد التجهيز","Shipped"=>"تم الشحن","Cancelled"=>"ملغي",_=>"تم التسليم"})+$" · {o.Total:0.00} EGP"))+T("\nDemo statuses are updated manually by the admin, not a live courier.","\nهذه حالات تجريبية يحدّثها المدير يدوياً وليست تتبعاً لشركة شحن.");
  }
  if(Has("coupon","discount","كوبون","خصم","كود"))
  {
   var active=(await shop.CouponsAsync()).Where(c=>c.IsActive && c.ExpiresAt>DateTime.UtcNow).Take(3);
   return string.Join("\n",active.Select(c=>$"{c.Code}: {c.Percent}% · "+T("minimum ","الحد الأدنى ")+$"{c.MinimumSpend:0.00} EGP")) is {Length:>0} text ? text+T("\nApply one code at checkout. Validity is checked again before ordering.","\nاستخدم كوداً واحداً عند الدفع. نتحقق منه مجدداً قبل تأكيد الطلب."):T("No active coupons right now.","لا توجد كوبونات سارية حالياً.");
  }
 if(Has("shipping","delivery","شحن","توصيل"))
  {
   var zones=(await shop.ZonesAsync()).Where(z=>z.IsActive).ToList();var st=await shop.SettingsAsync();
   var list=string.Join("\n",zones.Select(z=>$"{(ar?z.NameAr:z.Name)}: {z.Fee:0.00} EGP · "+(z.FreeThreshold>0?T($"free from {z.FreeThreshold:0.00} EGP",$"مجاني من {z.FreeThreshold:0.00} جنيه"):T("no free delivery","بدون توصيل مجاني"))+$" · {z.MinDays}-{z.MaxDays} "+T("days","أيام")));
   return list+T("\nThe fee is calculated at checkout from your governorate, after any coupon. Demo store: no real courier, estimates are not promises.","\nتُحسب المصاريف عند الدفع حسب المحافظة وبعد الكوبون. متجر تجريبي: لا يوجد شحن فعلي والمواعيد تقديرية.");
  }
  if(Has("return","refund","استرجاع","إرجاع","ارجاع","مرتجع"))return T("Demo store: returns and refunds are not processed. There is no real payment to refund. A returns policy and support channel must be configured before launch.","متجر تجريبي: لا ننفّذ استرجاعاً أو استرداد أموال. لا توجد مدفوعات حقيقية لاستردادها. يجب إعداد سياسة المرتجعات وقناة الدعم قبل الإطلاق.");
 if(Has("pay","card","cash","دفع","بطاق","استلام"))
  {
   var st=await shop.SettingsAsync();
   return T("Checkout is a simulation only. Do not enter real card information. No money is charged and no payment provider is connected.","الدفع محاكاة فقط. لا تدخل بيانات بطاقة حقيقية. لا يتم خصم أموال ولا توجد بوابة دفع متصلة.")+(st.CodEnabled?T($"\nCash on delivery (demo) is available: fee {st.CodFee:0.00} EGP"+(st.CodMaxOrder>0?$", orders up to {st.CodMaxOrder:0.00} EGP.":"."),$"\nالدفع عند الاستلام (تجريبي) متاح: رسوم {st.CodFee:0.00} جنيه"+(st.CodMaxOrder>0?$" للطلبات حتى {st.CodMaxOrder:0.00} جنيه.":".")):"");
  }
  var all=await products.SearchAsync(null);
  var words=q.Split(' ',StringSplitOptions.RemoveEmptyEntries).Where(s=>s.Length>2).ToArray();
  var found=all.Where(p=>words.Any(s=>p.Name.ToLowerInvariant().Contains(s)||p.NameAr.Contains(s))).Take(3).ToList();
  if(found.Count>0)return string.Join("\n",found.Select(p=>$"{p.LocalizedName(ar)} · {p.Price:0.00} EGP · "+T($"{p.Stock} in stock",$"المتوفر: {p.Stock}")+"\n"+p.LocalizedDescription(ar)));
  if(Has("product","recommend","catalog","منتج","ترشيح","منتجات","اقترح"))return T("Explore our desk, audio and display collections. Try asking 'headphones price' or 'monitor stock'. Use search and price sorting to find the right fit.","تصفح تشكيلات المكتب والصوتيات والشاشات. جرّب السؤال عن سعر السماعات أو مخزون الشاشة. استخدم البحث والترتيب بالسعر لاختيار الأنسب.");
  return T("I am a built-in store guide, not generative AI. I can help with products, coupons, your orders, demo delivery, returns and payment. Try one of the quick questions below.","أنا دليل متجر مدمج ولست ذكاءً توليدياً. أساعدك في المنتجات والكوبونات وطلباتك والشحن التجريبي والمرتجعات والدفع. جرّب أحد الأسئلة السريعة.");
 }
}
