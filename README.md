# TrueLine

<div dir="rtl">

سایت خبری با فرانت و بک جدا. کاربر خبر و عکس می‌نویسد، متن در SQL Server می‌ماند، فایل عکس در MinIO ذخیره می‌شود و فقط مسیر آن در دیتابیس است.

</div>

A news site with a separate frontend and backend. People publish a story and a photo. The text lives in SQL Server, the image file lives in MinIO, and the database stores only the image path.

<div dir="rtl">

## فارسی

TrueLine دو برنامه است:

- `FrontEnd` یک سایت Angular است. صفحه اول خبرهای منتشرشده را نشان می‌دهد، `/news/:id` یک مطلب را باز می‌کند و `/admin/new` فرم ثبت خبر است.
- `BackEnd` یک Web API با ASP.NET Core و Entity Framework Core است. جدول `News` متن خبر را نگه می‌دارد و جدول `NewsImages` مسیر عکس را. خود فایل داخل باکت MinIO به نام `trueline-news` است.

### پیش‌نیازها

- .NET 9
- Node.js و Angular CLI
- SQL Server
- MinIO روی `localhost:9000`

ابزار مایگریشن، یک بار:

</div>

```powershell
dotnet tool install --global dotnet-ef --version 9.0.19
```

<div dir="rtl">

### اجرا

تنظیمات را بسازید و رمز MinIO را خودتان بگذارید. این فایل در گیت نمی‌آید.

</div>

```powershell
copy BackEnd\appsettings.example.json BackEnd\appsettings.json
```

<div dir="rtl">

دیتابیس:

</div>

```powershell
cd BackEnd
dotnet ef database update
dotnet run --launch-profile http
```

<div dir="rtl">

API روی `http://localhost:5230` بالا می‌آید.

سایت، در یک ترمینال دیگر:

</div>

```powershell
cd FrontEnd
npm install
ng serve
```

<div dir="rtl">

سایت روی `http://localhost:4200` باز می‌شود.

</div>

## English

TrueLine is two apps:

- `FrontEnd` is an Angular site. `/` lists published stories, `/news/:id` opens one article, and `/admin/new` is the form for a new story.
- `BackEnd` is an ASP.NET Core Web API using Entity Framework Core. The `News` table stores the article. The `NewsImages` table stores the image path. The file itself is stored in the MinIO bucket `trueline-news`.

### Requirements

- .NET 9
- Node.js and the Angular CLI
- SQL Server
- MinIO listening on `localhost:9000`

Install the migration tool once:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.19
```

### Run

Create local settings and fill in your own MinIO password. That file is not committed.

```powershell
copy BackEnd\appsettings.example.json BackEnd\appsettings.json
```

Database and API:

```powershell
cd BackEnd
dotnet ef database update
dotnet run --launch-profile http
```

The API listens on `http://localhost:5230`.

Site, in a second terminal:

```powershell
cd FrontEnd
npm install
ng serve
```

The site opens at `http://localhost:4200`.
