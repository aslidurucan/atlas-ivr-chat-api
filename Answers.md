## Soru - Cevap ve Mimari Tercihler Dokümantasyonu

---

# 1. Asterisk Entegrasyonu

## Soru

Geliştirilen API'nin bir Asterisk sistemi ile nasıl entegre edileceğini açıklayınız. Çağrı nasıl karşılanır? API nasıl çağrılır? Ses nasıl oynatılır?

## Yanıt

Asterisk, gelen telefon çağrılarını karşılayan, yönlendiren ve yöneten açık kaynaklı bir PBX (Private Branch Exchange) yazılımıdır.

Geliştirilen .NET 8 Chat API ile Asterisk entegrasyonu aşağıdaki akış üzerinden gerçekleştirilebilir:

### 1. Çağrının Karşılanması (Answer)

Müşteri santrali aradığında Asterisk'in Dialplan mekanizması devreye girer ve:

```asterisk
Answer()
```

komutu ile çağrı karşılanır.

### 2. Ses Verisinin Alınması ve STT

Müşterinin konuşması alınır ve bir Speech-to-Text (STT) servisi tarafından metne dönüştürülür.

Örnek:

```text
"Fatura borcumu öğrenmek istiyorum."
```

↓

```text
Fatura borcumu öğrenmek istiyorum.
```

### 3. API'nin Tetiklenmesi

Asterisk tarafında çalışan bir AGI (Asterisk Gateway Interface) scripti elde edilen metni API'ye gönderir.

Örnek HTTP isteği:

```http
POST /api/chat
Content-Type: application/json
```

```json
{
  "message": "Fatura borcumu öğrenmek istiyorum."
}
```

### 4. Yapay Zeka Süreci

API gelen isteği karşılar ve Groq üzerinde çalışan Llama 3.1 modeline iletir.

Örnek cevap:

```json
{
  "response": "Fatura borcunuzu görüntülemek için müşteri numaranızı paylaşabilir misiniz?"
}
```

### 5. TTS ve Ses Oynatma

Asterisk tarafındaki AGI scripti response alanını okur.

Bu metin:

- Azure Neural TTS
- Piper TTS

gibi bir TTS motoru aracılığıyla sese dönüştürülür.

Sonrasında oluşturulan ses dosyası, Asterisk Dialplan tarafında:

​```asterisk
Playback()
​```

komutu ile; AGI scripti içerisinden ise:

​```text
STREAM FILE
​```

AGI komutu ile kullanıcıya dinletilir.

### Yanıt Süresi ve Kesme (Barge-in)

IVR deneyiminde STT + LLM + TTS zincirinin toplam gecikmesi doğrudan hissedilir. Bu nedenle LLM tarafında düşük gecikmeli Groq tercih edilerek toplam yanıt süresi düşük tutulmuştur. Ayrıca müşterinin anonsu dinlerken araya girip konuşabilmesi (barge-in) için Asterisk tarafında ilgili ses oynatma komutları kesilebilir biçimde yapılandırılabilir.

### Özet Akış

```text
Müşteri
   ↓
Asterisk Answer()
   ↓
STT (Speech To Text)
   ↓
AGI Script
   ↓
POST /api/chat
   ↓
Groq + Llama 3.1
   ↓
JSON Response
   ↓
TTS (Text To Speech)
   ↓
Playback()
   ↓
Müşteri
```

---

# 2. STT (Speech To Text)

## Soru

Kullanıcının sesi yazıya çevrilecek olsa hangi teknolojiyi tercih ederdiniz? Neden?

## Yanıt

STT tarafında tercih edeceğim teknoloji:

### OpenAI Whisper (faster-whisper)

Tercih sebepleri:

### Veri Gizliliği

Whisper tamamen lokal ortamda çalıştırılabilir.

Bu sayede:

- KVKK uyumluluğu sağlanır.
- Hassas müşteri verileri dış servislere gönderilmez.
- Telefon kayıtları kurum dışına çıkmaz.

### Türkçe Dil Başarısı

Whisper günümüzde Türkçe dilinde en başarılı açık kaynaklı STT modellerinden biridir.

Özellikle:

- Gürültülü telefon kayıtlarında
- Düşük kaliteli seslerde
- Farklı aksanlarda

yüksek doğruluk oranı sunar.

### Performans

faster-whisper sürümü:

- C++ optimizasyonları içerir.
- CPU üzerinde verimli çalışır.
- Gerçek zamanlı IVR senaryoları için uygundur.
- 
### Telefon Ses Kalitesi Uyumu

Telefon hatları dar bantlı ses taşır (genellikle 8kHz, μ-law / a-law kodlama). Whisper bu düşük örnekleme oranındaki ve gürültülü telefon kayıtlarında dahi yüksek doğruluk sağlayabildiği için IVR senaryosuna uygundur.

### Alternatifler

- Google Cloud Speech-to-Text
- Azure Speech Service

Bu servisler daha gelişmiş streaming özellikleri sunsa da:

- Sürekli maliyet oluştururlar.
- Verinin buluta çıkmasına neden olurlar.

---

# 3. TTS (Text To Speech)

## Soru

API cevabı ses olarak okutulacak olsa hangi teknolojiyi tercih ederdiniz? Neden?

## Yanıt

### Seçenek 1: Azure Neural TTS

Kurumsal projelerde ilk tercihim olurdu.

Avantajları:

- İnsan sesine çok yakın kalite
- Türkçe desteği
- Düşük gecikme
- SSML desteği

Örnek SSML:

```xml
<speak>
  <prosody rate="-10%">
    Merhaba, size nasıl yardımcı olabilirim?
  </prosody>
</speak>
```

### Seçenek 2: Piper TTS

Tamamen lokal çalışabilen açık kaynaklı alternatiftir.

Avantajları:

- Ücretsiz
- İnternet gerektirmez
- Çok düşük gecikme
- Kolay entegrasyon

Kurumsal gizlilik gereksinimlerinde güçlü bir seçenektir.

### Asterisk Format Uyumu

TTS motorundan elde edilen ses çıktısı, Asterisk'in beklediği formata (genellikle 8kHz mono WAV veya GSM) dönüştürülerek Playback aşamasına aktarılır. Bu dönüşüm, telefon hattı ses kalitesiyle uyumu sağlar.

---

# 4. Yapay Zeka

## Soru

Ücretsiz veya lokal çalışabilen hangi yapay zeka çözümünü tercih ederdiniz? Neden?

## Yanıt

Bu projede tercih ettiğim çözüm:

### Groq API + Llama 3.1 8B Instant

Tercih sebepleri:

### Düşük Gecikme

IVR sistemlerinde cevap süresi kritik öneme sahiptir.

Groq'un LPU altyapısı sayesinde:

- Çok hızlı token üretimi
- Düşük bekleme süresi
- Akıcı konuşma deneyimi

sağlanmaktadır.

### Maliyet Avantajı

- Ücretsiz kullanım
- Yüksek kota
- Geliştirme ve test süreçlerinde maliyet avantajı

sunmaktadır.

### Açık Kaynak Model

Llama 3.1:

- Güçlü Türkçe performansı
- Başarılı akıl yürütme
- Yüksek doğruluk

sunmaktadır.

### Güvenlik

API anahtarları:

- .NET User Secrets
- Environment Variables

üzerinden yönetilmektedir.

Kaynak kod içerisinde tutulmamaktadır.

### Lokal Alternatif

Tamamen internetten bağımsız bir çözüm gerekirse:

- Ollama
- Llama 3.1
- Mistral 7B

kombinasyonu tercih edilebilir.

Ancak yüksek donanım gereksinimi ve daha düşük performans dezavantajları bulunmaktadır.

---

# 5. Test Süreci

## Soru

Bu uygulamayı sıfırdan kuracak bir kişi için aşağıdaki adımları yazınız:

- Proje nasıl çalıştırılır?
- API nasıl test edilir?
- Asterisk ile nasıl test edilir?
- Hangi araçlar kullanılır?

## Yanıt

## A. Ön Gereksinimler

- Docker Desktop
- .NET 8 SDK (opsiyonel)
- Geçerli bir Groq API Key

---

## B. Projenin Çalıştırılması

### Yöntem 1: Docker ile (Önerilen)

#### Windows PowerShell

```powershell
$env:GROQ_API_KEY="KENDI_GROQ_API_ANAHTARINIZ"

docker compose up -d --build
```

#### Linux / Mac / Git Bash

```bash
export GROQ_API_KEY="KENDI_GROQ_API_ANAHTARINIZ"

docker compose up -d --build
```

Uygulama:

```text
http://localhost:8080
```

adresinden erişilebilir olacaktır.

---

### Yöntem 2: Lokal Çalıştırma

#### API Anahtarını Tanımlama

```bash
dotnet user-secrets set "GroqSettings:ApiKey" "KENDI_GROQ_API_ANAHTARINIZ" --project src/AtlasIvrChat.Api
```

#### Uygulamayı Başlatma

```bash
dotnet run --project src/AtlasIvrChat.Api
```

Varsayılan adres:

```text
http://localhost:5203
```

---

## C. API Testi

### Swagger UI

Docker:

```text
http://localhost:8080/swagger/index.html
```

Lokal:

```text
http://localhost:5203/swagger/index.html
```

### cURL Örneği

```bash
curl -X POST "http://localhost:8080/api/chat" \
-H "Content-Type: application/json" \
-d '{"message":"Merhaba"}'
```

### Beklenen Başarılı Yanıt

```json
{
  "response": "Merhaba! Size nasıl yardımcı olabilirim?"
}
```

### Validasyon Testi

Boş mesaj gönderildiğinde:

```json
{
  "message": ""
}
```

API'nin HTTP 400 Bad Request döndürmesi beklenir.

### Visual Studio HTTP Client

```text
src/AtlasIvrChat.Api/AtlasIvrChat.Api.http
```

dosyası kullanılarak IDE üzerinden test gerçekleştirilebilir.

---

## D. Asterisk ile Test Süreci

### Asterisk Kurulumu

- Docker üzerinde Asterisk
- VM üzerinde Asterisk

kurulabilir.

### SIP Kullanıcısı Tanımlama

Softphone örnekleri:

- Zoiper
- MicroSIP

### Dialplan Örneği

```asterisk
exten => 800,1,Answer()
same => n,AGI(ivr_chat_bridge.py)
same => n,Hangup()
```

### AGI Köprüsü

AGI scripti:

1. STT sonucunu alır.
2. API'ye gönderir.
3. Yanıtı TTS'e dönüştürür.
4. Playback ile kullanıcıya dinletir.

### Test Senaryosu

```text
Softphone
   ↓
Asterisk
   ↓
AGI Script
   ↓
Chat API
   ↓
Groq
   ↓
TTS
   ↓
Playback
   ↓
Softphone
```

800 numarası arandığında yapay zeka ile sesli görüşme doğrulanabilir.

---

## E. Kullanılan Araçlar

### Geliştirme

- .NET 8 SDK
- Visual Studio
- Docker
- Docker Compose

### API Testi

- Swagger UI
- Postman
- cURL
- Visual Studio HTTP Client


### LLM

- Groq API
- Llama 3.1 8B Instant