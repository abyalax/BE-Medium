## 1. Pub/Sub — Article Published Event

Goal: Saat artikel dipublish, beberapa service langsung menerima event yang sama.

Checklist:

- [x] Buat subject `article.published`
- [x] Buat Publisher (Article Service)
- [x] Publish event setelah artikel berhasil dibuat
- [x] Buat Notification Service sebagai subscriber
- [x] Buat Analytics Service sebagai subscriber
- [x] Buat Search Index Service sebagai subscriber
- [x] Verifikasi semua subscriber menerima event yang sama
- [x] Test dengan publish beberapa artikel

---

## 2. Request/Reply — Get Article

Goal: API Gateway meminta data artikel ke Article Service melalui NATS.

Checklist:

- [x] Buat subject `article.get`
- [x] Buat Requester (API Gateway)
- [x] Kirim request berisi `article_id`
- [x] Buat Responder (Article Service)
- [x] Ambil data artikel dari database/in-memory
- [x] Kirim reply ke requester
- [x] Tangani kasus artikel tidak ditemukan
- [x] Tambahkan timeout pada requester
- [x] Test request untuk ID valid dan invalid

---

## 3. JetStream (Pull Consumer) — AI Article Summarization

Goal: Artikel baru masuk ke stream, lalu AI Worker mengambil pekerjaan saat siap.

Checklist:

- [x] Enable JetStream
- [x] Buat Stream `ARTICLES`
- [x] Tambahkan subject `article.created`
- [x] Publish event setelah artikel dibuat
- [x] Buat Durable Pull Consumer
- [x] Buat AI Worker
- [x] Worker melakukan `Fetch()`
- [x] Generate ringkasan artikel (dummy atau AI)
- [x] Simpan hasil ringkasan
- [x] Kirim ACK setelah pekerjaan selesai
- [x] Test restart worker dan pastikan job belum ACK tetap diproses

- Pub/Sub: one event → many listeners.
- Request/Reply: synchronous communication antar service.
- JetStream Pull: reliable background job processing dengan persistence.

---

## 4. AI Summarization — ONNX Runtime GenAI

Goal: Gunakan model ONNX Phi-3 untuk generate summarization artikel.

Checklist:

- [x] Implementasi ONNX Runtime GenAI service
- [x] Integrasi dengan Phi-3 model dari Assets/Model
- [x] Update AiSummarizationWorker untuk menggunakan ONNX model
- [x] Konfigurasi AI settings di appsettings.json
- [x] Build dan testing berhasil
