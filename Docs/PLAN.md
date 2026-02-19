# Unity 2D Sahne ve Etkileşim Mimarisi Planı

## Neden önceden belirlemelisiniz?

Evet, **etkileşim kodlarını ve kurallarını önceden belirlemeniz şart**. Aksi halde:

- Bir geliştirici item'da `OnInteract()`, diğeri `Use()` yazarsa karakter hangisini çağıracak belli olmaz.
- Sahne A'da "Interactable", Sahne B'de "Item" tag'i kullanılırsa tek bir karakter script'i hepsini bulamaz.
- Merdiven mantığı bir sahnede `Trigger`, diğerinde `Collision` ile yapılırsa davranış tutarsız olur.

Bu yüzden **ortak sözleşmeler** (interface, tag, layer, isimlendirme) tek seferde tanımlanmalı; sahneler sadece bu sözleşmelere uyan objeleri yerleştirmeli.

---

## 1. Ortak arayüzler (interfaces)

| Sistem               | Interface önerisi                 | Karakterin yapacağı                                           |
| -------------------- | --------------------------------- | ------------------------------------------------------------- |
| **Item / etkileşim** | `IInteractable`                   | `Interact()` çağrısı, gerekiyorsa `GetPromptText()`           |
| **Merdiven**         | `IClimbable` veya merdiven alanı  | Tırmanma moduna geçiş (Move script'i devre dışı, dikey input) |
| **Boss / düşman**    | `IDamageable` (can, `TakeDamage`) | Hasar verme, ölüm event'i                                     |
| **Boss özel**        | `IBoss` (fazlar, pattern)         | İsteğe bağlı; `IDamageable`'dan türetilebilir                 |

Bu interface'ler **Assets/Scripts/Core/** altında tutulmalı.

---

## 2. Tag ve Layer standardı

**Tag'ler:** `Interactable`, `Ladder`, `Boss`, `Enemy`, `Hazard` (isteğe bağlı)

**Layer'lar:** Player, Enemy, Interactable, Ladder, Ground, OneWayPlatform — Edit > Project Settings > Tags and Layers'da tanımlanmalı.

---

## 3. Karakter tarafı

- Move script'i hareket/zıplama; ayrı bir **PlayerController** / **CharacterInteraction** script'i etkileşim + tırmanma + savaş.
- Bu script Core'daki interface ve tag'lere göre davranır; tüm sahnelerde aynı.

---

## 4. Prefab kullanımı

- Interactable, Ladder, Boss prefab'ları ortak; sahnede sadece yerleşim. Yeni davranış için önce Core'a ekle, sonra prefab güncelle.

---

## 5. Vibe coding için Structure

Yeni kod yazarken (AI veya elle) **önce Structure.md'yi oku**, sadece oradaki interface ve fonksiyonlarla yaz. Böylece üretilen kod tutarlı kalır.

**Yapılacaklar sırası:**

1. Structure dosyası oluştur (interface imzaları, tag/layer, karakter API).
2. Core klasörü: Structure'a uygun interface ve const'ları C# ile yaz.
3. Karakter: etkileşim + tırmanma script'i ekle (Structure/Core'a göre).
4. Prefab'lar ve SCENE_RULES.md.
5. Sahne tasarımında sadece prefab + Structure kuralları.
