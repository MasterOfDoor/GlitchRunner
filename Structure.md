# Structure — Vibe coding referansı

Yeni kod yazarken (AI veya elle) **önce bu dosyayı oku**. Sadece aşağıdaki interface imzaları, tag/layer ve karakter API'sini kullan; farklı isim veya imza kullanma.

---

## 1. Interface'ler (birebir kullanılacak imzalar)

### IInteractable
- `void Interact(GameObject interactor);`
- `string GetPromptText();`  // örn. "E - Al", "E - Tırman"

### IDamageable
- `void TakeDamage(int amount, GameObject source);`
- `bool IsAlive { get; }`

### IClimbable / Ladder
- Tırmanılabilir alan script'i: `bool CanClimb()` veya benzeri
- Karakter tırmanırken: dikey input, Move geçici devre dışı

### IBoss (isteğe bağlı)
- IDamageable'dan türetilebilir; fazlar / pattern için ek metodlar burada tanımlanır.

---

## 2. Tag sabitleri (exact string)

Kodda şu string'leri kullan (veya projede `GameTags` static sınıfında const olarak tanımla):

- `"Interactable"`
- `"Ladder"`
- `"Boss"`
- `"Enemy"`
- `"Hazard"` (isteğe bağlı)

Karakter script'i etkileşim tespiti için `CompareTag("Interactable")` vb. kullanacak.

---

## 3. Layer isimleri

Unity'de Tags and Layers'da tanımlanacak; raycast/collision için:

- Player, Enemy, Interactable, Ladder, Ground, OneWayPlatform

---

## 4. Karakter (Move) public API — kullanılacak metodlar

Etkileşim / tırmanma script'i Move'a erişirken **sadece** şu public üyeleri kullansın:

- `bool IsGrounded()`
- `Vector2 GetVelocity()`
- `float GetHorizontalSpeed()`
- `void SetMoveSpeed(float newSpeed)`
- `void SetJumpForce(float newForce)`
- `void AddImpulse(Vector2 impulse)`
- `void StopHorizontal()`

---

## 5. Etkileşim script'inin yapacağı çağrılar

- E tuşu + yakındaki obje: `IInteractable` varsa `Interact(playerGameObject)` ve gerekirse `GetPromptText()`.
- Merdiven alanı: Ladder tag veya IClimbable; tırmanma moduna geç, Move'u geçici kapat, dikey hareket uygula.
- Saldırı: `IDamageable` implement edenlere `TakeDamage(amount, playerGameObject)`.

---

## 6. İsimlendirme

- Script adları: **PascalCase** (örn. `PlayerInteraction`, `LadderZone`).
- Event / callback: **OnInteract**, **OnClimbEnter**, **OnDamageTaken**.

---

*Yeni özellik veya sahne objesi eklerken: önce Structure.md'yi oku, sonra sadece buradaki fonksiyon ve tag'lerle kod yaz.*
