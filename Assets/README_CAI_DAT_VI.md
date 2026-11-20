# DienGame – 5 ĐẢO CHƠI ĐƯỢC + CLIENT + TRANG BỊ (BẢN TEST UNITY)

## ĐÃ CÓ TRONG GÓI

- **5 đảo chọn được và vào được** trên cả Xạ thủ, Sát thủ và Pháp sư–Chimera (dùng lại ba scene chiến đấu của từng class để không nhân bản code).
- **Bốn đảo mới mở khóa để test**: 2 Rừng Thiên Mộc, 3 Cổ Tự Cát Vàng, 4 Hỏa Thành Long Mạch, 5 Cung Băng Vân Thiên. Đảo 1 Làng Phù Sa giữ gameplay gốc.
- **Bốn ảnh đảo** ở Lobby + bốn nền gameplay tile thực tế + 12 sprite prop (cây, cổ tự, nham thạch, băng và đá) với collision ở map ingame. Bố cục chướng ngại vật được random theo seed cố định của từng đảo; map chữ nhật và không tràn ra ngoài biên.
- **Bốn tab chức năng**: Trang Bị (3 slot nâng cấp tới +5 có cộng chỉ số), Kỹ Năng (xem bộ skill class), Nhiệm Vụ (giết 20 quái, nhận thưởng), Cửa Hàng (mua nguyên liệu và rương).
- **Loot có hình trong trận**: vàng và mảnh tinh thể rơi trên đất, nhặt khi đứng gần Player; lưu bằng PlayerPrefs.
- **Thưởng qua đảo**: vàng, đá và rương. Boss có HP tăng nhẹ theo độ khó đảo, dùng lại EnemyFactory và UI boss.
- **Code tự dựng map theo stage** qua `WorldStageRuntime`, code import sprite, code setup menu, không cần thao tác Inspector/Hierarchy thủ công.
- **Giữ nguyên** skill, nhân vật, EXP, joystick và AI của Xạ thủ/Sát thủ/Pháp sư–Chimera từ gói đang cài.

## CÀI – CÁCH 1: GÓI PATCH (khuyên dùng với project mày đang chơi)

1. **Backup project** (đặc biệt `Assets`, `ProjectSettings`).
2. Chép toàn bộ thư mục `Assets` trong ZIP patch vào gốc project Unity, đồng ý ghi đè.
3. Vào Unity, thoát Play Mode, đợi compile hết lỗi.
4. Chọn menu **`DienGame > World > 1 - Install ALL (5 Islands + Items + Lobby)`**.
5. Mở `Assets/_Game/Scenes/Lobby.unity`, bấm Play, chọn một class, chọn bất kỳ Đảo 1–5 rồi `VÀO TRẬN`.
6. Xem map gameplay. Bấm `VỀ SẢNH` ở pause menu để đổi đảo/class. Bấm tab `TRANG BỊ` / `NHIỆM VỤ` / `CỬA HÀNG` để test vật phẩm.

## CÀI – CÁCH 2: GÓI FULL ASSETS

Giải nén gói FULL ASSETS vào project Unity đã có `Packages/` và `ProjectSettings/`. Đây là **Assets hoàn chỉnh được dựng lại từ `Assets(8).zip` và các bản vá Pháp sư** đã dùng trong cuộc trò chuyện, cộng thêm 5 map mới. **Không thay thế project đã bị mày chỉnh sau các bản này mà không backup.** Chạy menu Installer ở bước 4 như trên.

## KIẾN TRÚC & ĐƯỜNG DẪN

- `Assets/_Game/Scripts/Gameplay/World/` – 5-stage runtime map builder, collider, ground, loot, item inventory, stat applier, stage intro.
- `Assets/_Game/Scripts/Client/WorldHubPanels.cs` – UI client Trang Bị, Kỹ Năng, Nhiệm Vụ, Cửa Hàng.
- `Assets/_Game/Scripts/Client/{LobbyUI,MetaProgressService,LobbyStageCatalog}.cs` – 5 map unlocked, names/rewards/navigation.
- `Assets/_Game/Scripts/Gameplay/Run/GameRunController.cs` – tăng HP boss và thưởng thắng mỗi đảo.
- `Assets/_Game/Scripts/Gameplay/Enemy/EnemyWaveSpawner.cs` – tránh sinh quái bên trong chướng ngại vật.
- `Assets/_Game/Resources/World/Stages/` – nền tile stage 2–5 (Stage 1 giữ Ground gốc).
- `Assets/_Game/Resources/World/Props/` – sprite vật cản với collider sinh từ code.
- `Assets/_Game/Resources/World/UI/` – icon vật phẩm rơi.
- `Assets/_Game/Editor/DienGameWorldInstaller.cs` – một menu cài đặt.

## LƯU Ý PHẠM VI

Đây là **vertical slice test thật** (có điều khiển, va chạm, quái, loot, lên cấp, boss, thắng/thua). Chưa phải phiên bản thương mại hoàn thiện: chưa có quest cốt truyện chi tiết từng đảo, A* navigation cho quái ở địa hình phức tạp, boss riêng cho mỗi đảo, công thức trang bị nhiều phẩm cấp, âm thanh/voiceover, nhiệm vụ trực tuyến, xác thực dữ liệu server. Các prop ingame dùng art nền và sprite thiết kế theo chủ đề; **ảnh đảo chỉ dùng ở Lobby**, không thay nền gameplay bằng hình đảo giả 3D.

**Quan trọng:** Gói được rà soát tĩnh nhưng **không thể chạy Unity Editor/Play Mode trong môi trường này**. Nếu Console xuất hiện compile error từ phiên bản Unity/package cục bộ, gửi kèm Console và file `.cs` liên quan để xử lý đúng bản.
