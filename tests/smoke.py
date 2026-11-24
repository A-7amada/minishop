import subprocess,time,os,re,json,sys,shutil,tempfile
from pathlib import Path
from playwright.sync_api import sync_playwright
source=Path(__file__).resolve().parents[1]
work=tempfile.TemporaryDirectory(prefix='minishop-test-')
root=str(Path(work.name)/'MiniShop')
shutil.copytree(source,root,ignore=shutil.ignore_patterns('bin','obj','*.db','*.db-*','test-output','.git'))
output=source/'test-output';output.mkdir(exist_ok=True)
dotnet=os.environ.get('DOTNET','dotnet')
chrome=os.environ.get('CHROME') or shutil.which('google-chrome') or shutil.which('chromium')
if not chrome:raise RuntimeError('Set CHROME to the Chrome or Chromium executable')
subprocess.run([dotnet,'build','--disable-build-servers'],cwd=root,check=True)
log=open(output/'server.log','w')
server=subprocess.Popen([dotnet,'run','--no-build','--project','src/MiniShop.Web','--urls','http://127.0.0.1:5000'],cwd=root,stdout=log,stderr=log,env={**os.environ,'ASPNETCORE_ENVIRONMENT':'Development'})
try:
 with sync_playwright() as pw:
  browser=pw.chromium.launch(executable_path=chrome,headless=True,args=['--no-sandbox'])
  ctx=browser.new_context(viewport={'width':1440,'height':1100});page=ctx.new_page()
  for i in range(80):
   try:page.goto('http://127.0.0.1:5000');break
   except:time.sleep(.3)
  errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  base='http://127.0.0.1:5000'
  def go(path):
   r=page.goto(base+path);assert r.status<400,(path,r.status,page.content()[:200]);return r
  def post(path,data={},source='/'):
   go(source);token=page.locator('input[name=__RequestVerificationToken]').first.get_attribute('value')
   r=ctx.request.post(base+path,form={**data,'__RequestVerificationToken':token});assert r.status<400,(path,r.status,r.text()[:600]);return r
  def login(email,password):
   post('/Account/Login',{'email':email,'password':password},'/Account/Login')
  def ask(q):return post('/Assistant/Ask',{'question':q}).json()['answer']
  assert 'built-in' in ask('hello there')
  assert 'Sign in' in ask('track my orders')
  assert '10%' in ask('discount codes')
  # anti-forgery and admin protection
  assert ctx.request.post(base+'/Cart/Add/1').status==400
  assert 'Login' in ctx.request.get(base+'/Admin/Shop/Coupons').url
  go('/?category=Audio');assert page.locator('.product').count()==1
  go('/?q=mouse');assert page.locator('.product').count()==1
  go('/?sort=low');assert 'Wireless Mouse' in page.locator('.product h3').first.inner_text()
  go('/');page.screenshot(path=str(output/'minishop4-home-en.png'),full_page=True)
  go('/Culture/Set?culture=ar&returnUrl=/');assert page.locator('html').get_attribute('dir')=='rtl'
  assert 'WELCOME10' in ask('هل فيه كوبون خصم')
  page.screenshot(path=str(output/'minishop4-home-ar.png'),full_page=True)
  go('/Culture/Set?culture=en&returnUrl=/')
  login('demo@minishop.local','Demo123!')
  post('/Wishlist/Toggle',{'id':'4','returnUrl':'/Wishlist'})
  go('/Wishlist');assert 'Noise-Cancelling Headphones' in page.locator('main').inner_text()
  page.screenshot(path=str(output/'minishop4-wishlist.png'),full_page=True)
  post('/Cart/Add/1');post('/Cart/Update',{'id':'1','quantity':'2'})
  go('/Checkout');assert '698.00' in page.locator('main').inner_text() # 2 mice at EGP 349
  post('/Checkout/Coupon',{'code':'welcome10'},'/Checkout')
  go('/Checkout');assert 'WELCOME10'==page.locator('#coupon').input_value()
  assert '-69.80' in page.locator('main').inner_text()
  A={'fullName':'Demo User','phone':'01012345678','city':'Nasr City','line':'12 Test Street, Apt 4'}
  def draft(region,pay='Mock'):post('/Checkout/Draft',{**A,'regionId':str(region),'payment':pay},'/Checkout');go('/Checkout')
  txt=lambda:page.locator('main').inner_text()
  # no governorate chosen: fee not shown, order button disabled
  assert page.locator('#pay-btn').is_disabled()
  # Greater Cairo: basket 628.20 >= 500 so delivery is free
  draft(1);assert 'FREE' in page.locator('#ship-fee').inner_text() and '628.20' in page.locator('#grand-total').inner_text()
  # Alexandria zone: 45 EGP, free from 750, basket 628.20 -> hint with 121.80 missing
  draft(4);assert '45.00' in page.locator('#ship-fee').inner_text() and '673.20' in page.locator('#grand-total').inner_text() and '121.80' in page.locator('#free-hint').inner_text()
  page.screenshot(path=str(output/'minishop4-checkout-shipping.png'),full_page=True)
  # Remote area: 90 EGP
  draft(23);assert '90.00' in page.locator('#ship-fee').inner_text() and '718.20' in page.locator('#grand-total').inner_text()
  # Upper Egypt zone: 60 EGP
  draft(15);assert '60.00' in page.locator('#ship-fee').inner_text()
  # Coupon apply keeps the typed address/region
  post('/Checkout/Coupon',{'code':'WELCOME10',**A,'regionId':'4','payment':'Mock'},'/Checkout');go('/Checkout');assert page.locator('#region').input_value()=='4' and page.locator('input[name=city]').input_value()=='Nasr City'
  # COD adds the configurable fee
  draft(4,'COD');assert '15.00' in page.locator('#cod-fee').inner_text() and '688.20' in page.locator('#grand-total').inner_text()
  page.screenshot(path=str(output/'minishop4-checkout-cod.png'),full_page=True)
  go('/Culture/Set?culture=ar&returnUrl=/Checkout');assert page.locator('html').get_attribute('dir')=='rtl';assert 'الدفع عند الاستلام' in txt()
  page.screenshot(path=str(output/'minishop4-checkout-ar.png'),full_page=True)
  go('/Culture/Set?culture=en&returnUrl=/Checkout')
  # server-side validation of the address
  r=post('/Checkout/Pay',{'fullName':'','phone':'abc','regionId':'1','city':'','line':'','payment':'Mock'},'/Checkout');assert 'complete your name' in r.text()
  r=post('/Checkout/Pay',{**A,'regionId':'0','payment':'Mock'},'/Checkout');assert 'select your governorate' in r.text()
  r=post('/Checkout/Pay',{**A,'regionId':'999','payment':'Mock'},'/Checkout');assert 'select your governorate' in r.text()
  # first order: Cairo, demo payment (free delivery) so totals stay 628.20
  result=post('/Checkout/Pay',{**A,'regionId':'1','payment':'Mock'},'/Checkout');assert 'Order placed' in result.text() or 'Order Placed' in result.text()
  assert 'Paid (demo)' in result.text()
  go('/Account/Orders');assert '628.20' in page.locator('main').inner_text()
  assert '628.20' in ask('track my orders')
  # chat widget real UI
  go('/');page.locator('#chat-toggle').click();page.locator('#chat-question').fill('Any discount codes?');page.locator('#chat-form button').click();page.wait_for_function("document.querySelectorAll('.bubble.bot').length>1")
  page.screenshot(path=str(output/'minishop4-assistant.png'),full_page=True)
  # desktop and Arabic mobile overflow
  page.set_viewport_size({'width':390,'height':844});go('/Culture/Set?culture=ar&returnUrl=/');assert page.evaluate('document.documentElement.scrollWidth<=innerWidth')
  page.screenshot(path=str(output/'minishop4-mobile-ar.png'),full_page=True)
  page.locator('#chat-toggle').click();page.locator('#chat-question').fill('تتبع طلباتي');page.locator('#chat-form button').click();page.wait_for_function("document.querySelectorAll('.bubble.bot').length>1");assert 'تم التأكيد' in page.locator('.bubble.bot').last.inner_text()
  page.screenshot(path=str(output/'minishop4-assistant-ar.png'),full_page=True)
  page.set_viewport_size({'width':1440,'height':1100});go('/Culture/Set?culture=en&returnUrl=/')
  post('/Account/Logout');login('admin@minishop.local','Admin123!')
  go('/Admin/Products');go('/Admin/Products/Create');go('/Admin/Products/Edit/1');go('/Admin/Shop/Coupons');assert 'WELCOME10' in page.locator('main').inner_text()
  post('/Admin/Shop/SaveCoupon',{'Code':'MINTEST','Percent':'15','MinimumSpend':'999999','ExpiresAt':'2030-01-01T00:00','IsActive':'true'},'/Admin/Shop/Coupons')
  post('/Admin/Shop/SaveCoupon',{'Code':'EXPIRED','Percent':'15','MinimumSpend':'0','ExpiresAt':'2020-01-01T00:00','IsActive':'true'},'/Admin/Shop/Coupons')
  go('/Admin/Shop/Coupons');page.screenshot(path=str(output/'minishop4-admin-coupons.png'),full_page=True)
  go('/Admin/Shop/Shipping');assert 'Greater Cairo' in page.locator('main').inner_text() and 'Cash on delivery' in page.locator('main').inner_text()
  page.screenshot(path=str(output/'minishop4-admin-shipping.png'),full_page=True)
  def zone(i,name,ar,fee,free,mn,mx,act='true'):post('/Admin/Shop/SaveZone',{'Id':str(i),'Name':name,'NameAr':ar,'Fee':str(fee),'FreeThreshold':str(free),'MinDays':str(mn),'MaxDays':str(mx),'IsActive':act},'/Admin/Shop/Shipping')
  zone(2,'Alexandria, Delta & Canal','الإسكندرية والدلتا والقناة',50,750,2,3);go('/Admin/Shop/Shipping');assert page.locator('input[name=Fee]').nth(1).input_value()=='50'
  r=post('/Admin/Shop/SaveZone',{'Id':'2','Name':'X','NameAr':'Y','Fee':'-5','FreeThreshold':'0','MinDays':'3','MaxDays':'1','IsActive':'true'},'/Admin/Shop/Shipping');assert 'Check the code' in r.text()
  zone(2,'Alexandria, Delta & Canal','الإسكندرية والدلتا والقناة',45,750,2,3)
  post('/Admin/Shop/SetRegion',{'regionId':'11','zoneId':'1'},'/Admin/Shop/Shipping');post('/Admin/Shop/SetRegion',{'regionId':'11','zoneId':'2'},'/Admin/Shop/Shipping')
  post('/Admin/Shop/SaveSettings',{'CodEnabled':'true','CodFee':'15','CodMaxOrder':'20000'},'/Admin/Shop/Shipping')
  post('/Admin/Shop/Status',{'id':'1','status':'Processing'},'/Admin/Shop/Orders')
  go('/Admin/Shop/Orders');assert 'Processing' in page.locator('main').inner_text()
  rejected=post('/Admin/Shop/Status',{'id':'1','status':'Delivered'},'/Admin/Shop/Orders')
  assert 'Only the next' in rejected.text()
  post('/Account/Logout');login('demo@minishop.local','Demo123!')
  go('/Account/Orders');page.screenshot(path=str(output/'minishop4-orders.png'),full_page=True)
  assert 'Processing' in ask('track order 1')
  go('/Home/Details/1');assert '23 in stock' in page.locator('main').inner_text()
  # COD order: Alexandria, 1 mouse 349 + 45 shipping + 15 COD fee = 409.00 (coupon was consumed by the first order)
  post('/Cart/Add/1');post('/Checkout/Draft',{**A,'regionId':'4','payment':'COD'},'/Checkout');go('/Checkout')
  assert '409.00' in page.locator('#grand-total').inner_text()
  # Admin lowers the COD cap below this order: COD is refused server-side, demo payment still works
  post('/Account/Logout');login('admin@minishop.local','Admin123!');post('/Admin/Shop/SaveSettings',{'CodEnabled':'true','CodFee':'15','CodMaxOrder':'100'},'/Admin/Shop/Shipping')
  post('/Account/Logout');login('demo@minishop.local','Demo123!');post('/Checkout/Draft',{**A,'regionId':'4','payment':'COD'},'/Checkout');go('/Checkout')
  assert 'cash on delivery limit' in txt() and '394.00' in page.locator('#grand-total').inner_text()
  r=post('/Checkout/Pay',{**A,'regionId':'4','payment':'COD'},'/Checkout');assert 'cash on delivery limit' in r.text()
  post('/Account/Logout');login('admin@minishop.local','Admin123!')
  post('/Admin/Shop/SaveSettings',{'CodEnabled':'false','CodFee':'15','CodMaxOrder':'20000'},'/Admin/Shop/Shipping')
  post('/Account/Logout');login('demo@minishop.local','Demo123!')
  r=post('/Checkout/Pay',{**A,'regionId':'4','payment':'COD'},'/Checkout');assert 'not available right now' in r.text()
  post('/Account/Logout');login('admin@minishop.local','Admin123!')
  post('/Admin/Shop/SaveSettings',{'CodEnabled':'true','CodFee':'15','CodMaxOrder':'20000'},'/Admin/Shop/Shipping')
  post('/Account/Logout');login('demo@minishop.local','Demo123!')
  result=post('/Checkout/Pay',{**A,'regionId':'4','payment':'COD'},'/Checkout')
  assert 'Due on delivery' in result.text() and '409.00' in result.text()
  go('/Home/Details/1');assert '22 in stock' in txt()
  go('/Account/Orders');assert 'Cash on delivery' in txt() and '12 Test Street' in txt()
  page.screenshot(path=str(output/'minishop4-orders-cod.png'),full_page=True)
  assert 'Due on delivery' in ask('track my orders') or '409.00' in ask('track my orders')
  assert '45.00' in ask('how does delivery work') and 'Greater Cairo' in ask('how does delivery work')
  # customer cancels the still-Confirmed COD order: stock returns
  post('/Account/Cancel/2',{},'/Account/Orders');go('/Account/Orders');assert 'Cancelled' in txt()
  go('/Home/Details/1');assert '23 in stock' in txt()
  r=post('/Account/Cancel/2',{},'/Account/Orders');assert 'Only orders that are still Confirmed' in r.text()
  r=post('/Account/Cancel/1',{},'/Account/Orders');assert 'Only orders' in r.text()  # order 1 is already Processing
  # order 2 is cancelled: admin cannot move it forward
  post('/Account/Logout');login('admin@minishop.local','Admin123!')
  r=post('/Admin/Shop/Status',{'id':'2','status':'Confirmed'},'/Admin/Shop/Orders');assert 'Only the next' in r.text()
  post('/Account/Logout');login('demo@minishop.local','Demo123!')
  # COD order lifecycle: fresh COD order, admin delivers, payment becomes Collected
  post('/Cart/Add/3');post('/Checkout/Draft',{**A,'regionId':'1','payment':'COD'},'/Checkout')
  post('/Checkout/Pay',{**A,'regionId':'1','payment':'COD'},'/Checkout')
  post('/Account/Logout');login('admin@minishop.local','Admin123!')
  for st in ('Processing','Shipped','Delivered'):post('/Admin/Shop/Status',{'id':'3','status':st},'/Admin/Shop/Orders')
  post('/Account/Logout');login('demo@minishop.local','Demo123!')
  go('/Account/Orders');assert 'Collected (demo)' in txt()
  assert page.locator('form.cancel-form').count()==0
  post('/Cart/Add/2')
  r=post('/Checkout/Coupon',{'code':'MINTEST'},'/Checkout');assert 'below' in r.text()
  r=post('/Checkout/Coupon',{'code':'EXPIRED'},'/Checkout');assert 'expired' in r.text()
  r=post('/Checkout/Coupon',{'code':'NONSENSE'},'/Checkout');assert 'invalid' in r.text()
  # Other user's order cannot be exposed through assistant.
  post('/Account/Logout');post('/Account/Register',{'fullName':'Test Customer','email':'private@test.local','password':'Strong123!'},'/Account/Register')
  assert 'No matching' in ask('track order 1')
  assert 'WELCOME10' not in ask('track order 1')
  go('/Account/Orders');assert 'No orders' in page.locator('main').inner_text()
  assert not errors,errors
  print('ALL CHECKS PASSED; browser console clean.')
  browser.close()
finally:
 server.terminate();server.wait(timeout=10);log.close();work.cleanup()
