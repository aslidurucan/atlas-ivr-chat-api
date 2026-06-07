# 📞 AI-Powered IVR Chat API

Bu proje, .NET 8 mimarisi kullanılarak geliştirilmiş, yapay zeka destekli bir Sesli Yanıt (IVR) Chat API uygulamasıdır [source: 1]. Proje, kurumsal yazılım standartları gözetilerek **Clean Architecture** prensiplerine uygun olarak katmanlandırılmış ve tamamen **Dockerize** edilmiştir.

---

## 🏗️ Mimari Yapı ve Teknik Tercihler (Technical Decisions)

Projede iş kurallarının (business rules) ve çekirdek mantığın teknolojik bağımlılıklardan izole edilmesi amacıyla **Clean Architecture** ve **DDD (Domain-Driven Design)** yaklaşımları benimsenmiştir.

* **AtlasIvrChat.Domain:** Projenin kalbidir. Hiçbir dış kütüphane veya katman bağımlılığı barındırmaz. Yapay zeka servis kontratı (`IAiService`) ve veri modelleri (`ChatRequest`, `ChatResponse`) bu katmanda izole edilmiştir.
* **AtlasIvrChat.Infrastructure:** Harici servis entegrasyonlarının çözüldüğü katmandır. Yapay zeka sağlayıcısı olarak **Groq API (`llama-3.1-8b-instant`)** entegrasyonu bu katmanda somutlaştırılmıştır.
* **AtlasIvrChat.Api:** HTTP isteklerini karşılayan, istek validasyonlarını (Attribute-based Validation) yürüten ve merkezi hata yönetimini (`ExceptionHandlingMiddleware`) barındıran sunum katmanıdır.

### Neden Groq API & Llama 3.1?
IVR senaryolarında milisaniyeler seviyesindeki yanıt süreleri (low latency) müşteri deneyimi için kritiktir. Groq mimarisi, çıkarım (inference) hızında endüstri lideri olduğu ve açık kaynaklı güçlü `llama-3.1-8b-instant` modelini ücretsiz/yüksek kotalı sunduğu için tercih edilmiştir.

---

## 🐋 Docker ve Güvenlik Altyapısı

* **Multi-Stage Build:** `Dockerfile` mimarisinde derleme (SDK) ve çalışma zamanı (Runtime) katmanları ayrılarak minimum imaj boyutu ve maksimum güvenlik (reduced attack surface) elde edilmiştir.
* **Zero-Leak Secret Management:** Canlı API anahtarları (`ApiKey`) kaynak koda veya `appsettings.json` dosyasına gömülmemiştir. Yerel ortamdaki `.NET Secret Manager` yapısının Docker konteynerine güvenli aktarılması için **Volume Mount (Read-Only)** yöntemi tercih edilmiştir. Böylece `docker-compose.yml` şifresiz ve güvenli bir şekilde sürümlendirilebilmiştir.

---

## 🛠️ Kurulum ve Çalıştırma Rehberi (Test Süreci)

Bu projeyi yerel ortamınızda test etmek ve Docker üzerinde ayağa kaldırmak için aşağıdaki adımları sırasıyla uygulayınız.

### 1. Ön Gereksinimler
* Bilgisayarınızda **Docker Desktop**'ın kurulu ve çalışır durumda olması gerekmektedir.
* API'nin yapay zeka yanıtları üretebilmesi için ücretsiz bir **Groq API Key** gereklidir. (Kendi anahtarınızı [Groq Console](https://console.groq.com/) üzerinden ücretsiz oluşturabilirsiniz).

### 2. Groq API Key Tanımlama (.NET Secret Manager)
Projenin "Fail-Fast" mimari prensibi gereği, API Key tanımlanmadan sistem ayağa kalkmayacaktır. Bilgisayarınızda bir terminal açarak projenin kök dizinine gidiniz ve aşağıdaki komutla kendi API anahtarınızı yerel sisteminize güvenli bir şekilde tanımlayınız:

```bash
# Proje kök dizinindeyken API katmanına gizli anahtarınızı ekleyin
dotnet user-secrets set "GroqSettings:ApiKey" "YOUR_GROQ_API_KEY_HERE" --project src/AtlasIvrChat.Api
```

### 🚀 Projeyi Çalıştırma
Docker ile Çalıştırma

Gerekli ortam değişkenlerini ve yapılandırmaları tamamladıktan sonra, proje kök dizininde aşağıdaki komutu çalıştırın:
```bash
docker compose up -d --build
```
Bu komut:

Docker imajlarını oluşturur.
Gerekli servisleri başlatır.
Uygulamayı arka planda çalıştırır.

Uygulama başarıyla başlatıldıktan sonra aşağıdaki adresten erişilebilir olacaktır:

http://localhost:8080
---

## 🧪 API Testi

Uygulama ayağa kalktıktan sonra Swagger UI otomatik olarak kullanılabilir olacaktır.

### Swagger UI

```text
http://localhost:8080/swagger/index.html
```

Swagger UI üzerinden tüm endpoint'leri görüntüleyebilir ve doğrudan test edebilirsiniz.

---

### 📬 Postman veya cURL ile Test

#### Endpoint

```http
POST http://localhost:8080/api/chat
```

#### Headers

```http
Content-Type: application/json
```

#### Request Body

```json
{
  "message": "Merhaba"
}
```

#### cURL Örneği

```bash
curl -X POST "http://localhost:8080/api/chat" \
-H "Content-Type: application/json" \
-d '{
  "message": "Merhaba"
}'
```

#### Örnek Response

```json
{
  "response": "Merhaba! Size nasıl yardımcı olabilirim?"
}
```
