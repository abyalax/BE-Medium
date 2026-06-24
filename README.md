# 📝 .NET & EF Core Command Documentation

## 1. Entity Framework Core (Database Schema Management)

### Migrate UP (Menerapkan Perubahan Skema)

Digunakan untuk membuat database baru atau memperbarui skema database saat ini ke versi migrasi paling akhir.

```bash
# Menerapkan seluruh migrasi yang belum berjalan
dotnet ef database update

# Menerapkan migrasi hanya sampai pada file migrasi spesifik tertentu
dotnet ef database update [NamaMigrasiSpesifik]

```

### Migrate DOWN (Rollback / Membatalkan Migrasi)

Di EF Core, membatalkan migrasi dilakukan dengan mengarahkan target *update* ke nama file migrasi masa lalu yang ingin dituju.

```bash
# Mundur/rollback ke migrasi spesifik (menghapus tabel di atasnya)
dotnet ef database update [NamaMigrasiSebelumnya]

# Reset total: rollback semua migrasi sampai database kosong bersih
dotnet ef database update 0

```

### Tambah & Hapus File Migrasi (C# Code Generation)

```bash
# Membuat file migrasi baru setelah mengubah model/configuration C#
dotnet ef migrations add [NamaMigrasiBaru]

# Menghapus file migrasi TERAKHIR yang belum sempat di-apply ke database
dotnet ef migrations remove

```

---

## 2. Data Seeding Commands (Custom Script)

Gunakan perintah ini untuk memicu eksekusi pembuat data *mock* deterministik yang sudah kita pisah ke dalam script runner.

```bash
# Menjalankan manual seeding data (otomatis menjalankan migrasi terlebih dahulu jika ada)
dotnet run -- --seed

# Kombinasi: Reset total database fisik (Drop) + Migrate Up + Seed Data baru dalam 1 baris
dotnet ef database drop --force && dotnet run -- --seed

```

---

## 3. Development Environment Commands

### Dev Watch (Hot Reload)

Mengecek perubahan kode C# secara langsung tanpa perlu melakukan restart aplikasi manual (*live-reloading*). Sangat berguna saat membuat API.

```bash
# Menjalankan aplikasi dengan fitur hot-reload otomatis
dotnet watch run

# Menjalankan dev watch dengan profil launchSettings tertentu (misal: https/http)
dotnet watch run --launch-profile "https"

```

---

## 4. Build, Compile, & Maintenance Commands

### Build & Compile

```bash
# Melakukan kompilasi kode program untuk mengecek error sintaks/compiler
dotnet build

# Melakukan kompilasi bersih dengan menghapus cache build sebelumnya
dotnet build --no-incremental

```

### Run (Normal Mode)

```bash
# Menjalankan aplikasi web server secara normal (tanpa seeder & tanpa watch)
dotnet run

```

### Clean & Restore Dependencies

```bash
# Mengunduh ulang paket NuGet yang hilang atau belum terinstal
dotnet restore

# Menghapus folder otomatis bin/ dan obj/ hasil kompilasi terdahulu
dotnet clean

```
---