# راه‌اندازی TrueLine روی یک سیستم تازه

کلون کردن از گیت فقط کد را می‌آورد. دیتابیس، MinIO، پکیج‌ها و فایل تنظیمات محلی همراهش نیستند. ساختن کاربر به API روی `http://localhost:5230` و دیتابیس SQL Server وصل است. اگر یکی از این دو بالا نباشد، فرم ثبت‌نام خطا می‌دهد.

## ۱. این‌ها را نصب کن

- **.NET 9 SDK** (نه فقط Runtime). در PowerShell با `dotnet --version` باید عددی مثل `9.x` ببینی.
- **Node.js نسخه 22.22.3 یا بالاتر** (یا 24.15 به بالا). Angular این پروژه با Node قدیمی بالا نمی‌آید. با `node -v` چک کن.
- **SQL Server** روی همان ویندوز، با یک instance پیش‌فرض. رشته اتصال پروژه این است:

  `Server=.;Database=TrueLine;Trusted_Connection=True;TrustServerCertificate=True`

  یعنی SQL Server باید با نام `.` در دسترس باشد و کاربر ویندوز فعلی حق ورود داشته باشد. SQL Server Express با instance پیش‌فرض (`MSSQLSERVER`) این شرط را دارد. اگر فقط LocalDB داری، در قدم ۳ رشته اتصال را عوض کن.
- **MinIO** که روی `localhost:9000` گوش بدهد. برای ساختن کاربر لازم نیست، ولی برای عکس خبر و آواتار لازم است.

یک بار ابزار مایگریشن را نصب کن:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.19
```

## ۲. پروژه را کلون کن

```powershell
git clone <آدرس-ریپو>
cd TrueLine
```

## ۳. فایل تنظیمات را بساز

`BackEnd/appsettings.json` داخل گیت نیست. بدون آن API یا اصلاً بالا نمی‌آید، یا به دیتابیس وصل نمی‌شود.

```powershell
copy BackEnd\appsettings.example.json BackEnd\appsettings.json
```

فایل را باز کن و `AccessKey` و `SecretKey` را با همان یوزر و رمزی که خود MinIO را با آن اجرا می‌کنی پر کن. این دو مقدار همان `MINIO_ROOT_USER` و `MINIO_ROOT_PASSWORD` هستند.

اگر SQL Server تو instance پیش‌فرض نیست و LocalDB است، `DefaultConnection` را این‌طور بگذار:

```text
Server=(localdb)\MSSQLLocalDB;Database=TrueLine;Trusted_Connection=True;TrustServerCertificate=True
```

## ۴. دیتابیس را بساز

SQL Server باید در حال اجرا باشد. از Services، سرویس `SQL Server (MSSQLSERVER)` را روی Running بگذار. بعد:

```powershell
cd BackEnd
dotnet ef database update
```

این دستور دیتابیس `TrueLine` و جدول کاربران را می‌سازد. تا این دستور بدون خطا تمام نشده، ثبت‌نام کار نمی‌کند.

## ۵. MinIO را بالا بیاور

عکس‌ها داخل دیتابیس نیستند. فقط مسیرشان در SQL است و خود فایل در پوشه `C:\minio-data` می‌ماند. هر بار باید همان پوشه و همان رمز بالا بیاید. رمز را عوض نکن و پوشه داده جدید نساز، وگرنه عکس‌های قبلی دیگر باز نمی‌شوند.

رمز فقط در `BackEnd\appsettings.json` است. برنامه و MinIO باید همان `AccessKey` و `SecretKey` را داشته باشند. روی این سیستم کاربر `trueline` است. اگر MinIO از قبل با این پوشه ساخته شده باشد، رمز اولیه‌اش را نگه می‌دارد و با عوض کردن فایل تنظیمات عوض نمی‌شود.

```powershell
.\start-minio.ps1
```

این دستور MinIO را روی `localhost:9000` با همان رمز فایل تنظیمات اجرا می‌کند. کنسولش روی `http://localhost:9001` است. اگر پورت ۹۰۰۰ از قبل باز باشد، دوباره اجرا نمی‌شود.

## ۶. API را اجرا کن و این ترمینال را باز بگذار

```powershell
cd BackEnd
dotnet run --launch-profile http
```

باید ببینی که روی `http://localhost:5230` گوش می‌دهد. این پنجره را نبند.

## ۷. سایت را در یک ترمینال دیگر اجرا کن

```powershell
cd FrontEnd
npm install
npm start
```

سایت روی `http://localhost:4200` باز می‌شود. آدرس API داخل کد ثابت است: `http://localhost:5230`. فرانت و بک باید هر دو همزمان روشن باشند.

Angular CLI به‌صورت سراسری لازم نیست. `npm start` از نسخه داخل پروژه استفاده می‌کند.

## ۸. کاربر را بساز

هر دو سرور که بالا بودند، از `http://localhost:4200` ثبت‌نام کن. کاربر در جدول `Users` دیتابیس `TrueLine` ذخیره می‌شود. کاربران سیستم قبلی همراه کلون نمی‌آیند، چون دیتابیس داخل گیت نیست.

## اگر فرم ثبت‌نام خطا داد

در مرورگر DevTools را باز کن (F12) و تب Network را ببین:

- درخواست به `http://localhost:5230/api/auth/register` اصلاً نمی‌رود یا `Failed to fetch` است: API روشن نیست، یا با پروفایل `http` اجرا نشده.
- پاسخ `500` است: تقریباً همیشه SQL Server در دسترس نیست یا `dotnet ef database update` اجرا نشده. متن خطا را در همان ترمینال `dotnet run` بخوان.
- خطای قرمز موقع `npm install` یا `npm start` با عبارت engine یا Node: نسخه Node پایین است. Node 22.22.3 یا جدیدتر نصب کن، ترمینال را ببند و دوباره باز کن.
