# Color Block Jam - Technical Case Study

## Gameplay Demo

<p align="center">
  <a href="./gameplay-demo.mp4">
    <strong>▶ Watch Gameplay Demo</strong>
  </a>
</p>


<video src="./Media/gameplay-demo.mp4" controls width="100%"></video>

Bu projede, mobil bulmaca mekaniklerini olabildiğince temiz ve geliştirilmeye açık bir kod yapısıyla birleştirmeye çalıştım. Gereksiz karmaşadan kaçınarak, sistemlerin birbiriyle uyumlu çalışması için **State Machine** ve **Singleton Pattern** yaklaşımlarını kullandım.

## Merkezi Manager Sistemleri

Oyunun tüm kontrolü, her biri ayrı bir sorumluluğa sahip olan Manager scriptleri üzerinden dönüyor:

### GameManager

Oyunun ana beyni. Oyunun o anki durumunu (*State*) takip ediyor, levellerin sırayla yüklenmesini ve kazanma/kaybetme logic'lerinin tetiklenmesini yönetiyor. Tüm akış buradan kontrol ediliyor.

### GridManager

Projenin matematiksel temeli. Grid hücrelerini (CellNode) oluşturuyor, koordinat dönüşümlerini yapıyor ve kamerayı 9:16 portrait formatına göre otomatik sığdırıyor.

### UIManager

Oyun içi arayüzün dinamik tarafı. State değişimlerini dinleyerek *Start, Play, Level Complete* ve *Game Complete* ekranlarını yönetiyor.

### BlockInteractionManager

Oyuncunun input'larını (dokunma/sürükleme) alıp anlamlandırıyor. Bloğun ne kadar sürüklendiğine bakarak "çıkış kapısına vakumlanma" kararını veriyor.

## Teknik Çözümler

### Sweep Test Collision

L-Shape veya Cross gibi karmaşık blokların birbirinin içinden geçmesini (hayalet blok hatası) engellemek için özel bir **Sweep Test** algoritması yazdım. Bir blok çıkışa yöneldiğinde, sistem sadece ucunun değmesine bakmıyor; bloğun tüm parçalarını o yöne doğru sanal olarak süpürerek (sweep) yolun tamamen temiz olup olmadığını simüle ediyor.

### Object Pooling

Mobil tarafta performans çok kritik olduğu için **BlockPoolManager** yapısını kurdum. Sürekli Instantiate/Destroy yaparak işlemciyi yormak yerine, blokları bir pool içerisinde tutup ihtiyaç duyulduğunda tekrar aktif ederek bellek yönetimini (memory management) optimize ettim.

### Level Management & State Flow

Seviye geçişleri ve oyunun genel akışı merkezi bir **State Machine** üzerinden yürüyor. Bu sayede "Level bitti mi?", "UI kapandı mı?" gibi kontroller birbirine girmiyor. Seviyeler bittiğinde sistem otomatik olarak bir sonraki index'e geçiyor veya tüm leveller bittiyse *Game Complete* ekranını tetikliyor.

### Visual Polish & Selection UX (Outline)

Oyuncuya dokunsal geri bildirim vermek için blok seçildiğinde devreye giren performans dostu bir **Outline** sistemi entegre ettim. Derinlik kapanmasını (depth occlusion) önleyen shader ayarları kullandım. Ayrıca düz Orthographic kamera yerine 65 derecelik **Perspective kameraya** geçerek ve ortam ışıklandırmasını (Soft Shadows, Warm Ambient) optimize ederek blokların 3D hacmini ve hibrit-casual derinlik hissiyatını ön plana çıkardım.

## 3. Seviye Tasarımı (Level Design Tools)

### LevelData (ScriptableObject)

Seviye tasarımlarını (grid size, blok pozisyonları, kapı renkleri) koddan bağımsız birer *Asset* olarak tutuyorum. Bu sayede yeni bir level eklemek için koda dokunmadan sadece yeni bir data dosyası oluşturmak yetiyor.

### LevelVisualizer

Seviye tasarımını hızlandırmak için bir Editör aracı yazdım. Play moduna girmeden, sadece sahne ekranında **Gizmos** yardımıyla tasarladığım levelleri anlık görebiliyorum.

## ÇOK ÖNEMLİ: RESET KISAYOLU <strong><span style="color:#ee0000">("R" TUŞU)</span></strong>

Oyunun progresyonunu (kaydedilmiş seviye ilerlemesini) ve tüm testleri hızlıca sıfırlayabilmeniz için bir kısayol ekledim. Oyun açıkken klavyeden **"R"** tuşuna basarsanız, tüm kayıtlı veriler (PlayerPrefs) anında silinir ve oyun en baştan (Level 1) başlar. Test yaparken size vakit kazandıracaktır.

## 4. Proje Notları

- **Unity Sürümü:** 2022.3.53f1
- **Platform:** Windows (9:16 Portrait Mode simülasyonu olarak paketlendi).
- **Repo:** Gereksiz dosyalar (Library, Logs vb.) .gitignore ile elenmiştir, projeyi çektiğiniz an sorunsuz çalışacaktır.
