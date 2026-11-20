# DienGame — Bản bổ sung Trang bị & Cường hóa v1

Bản này được thêm trực tiếp trên cấu trúc code của `Assets(10).zip`, không thay scene hoặc prefab cũ. Hệ thống được dựng bằng code trong màn **TRANG BỊ** của Lobby; không phải một ảnh UI giả lập.

## Cài đặt

**Lựa chọn A — Full:** đóng Unity, sao lưu dự án, giải nén `DienGame_Assets_TrangBi_v1_FULL.zip` vào **thư mục gốc của project Unity** (thư mục cùng cấp với `Assets` và `ProjectSettings`). Cho phép ghi đè các file được thay thế. Không sao chép thành `Assets/Assets` lần nữa nếu Unity project của bạn đã có thư mục `Assets`.

**Lựa chọn B — Patch:** làm y hệt với `DienGame_TrangBi_v1_PATCH.zip`. Bản Patch nhỏ, chỉ chứa file mới và các script cần thay, phù hợp nếu máy bạn đã có đúng nội dung của `Assets(10).zip`.

**Không chạy các script `CAI_BAN_SUA.cmd`, `Install_DienGame_OneClick.ps1`, hoặc `World Installer` để cài tính năng này**. Tính năng mới tự được gắn theo cơ chế scene runtime sẵn có.

## Kiểm tra sau khi import

1. Đợi Unity import asset và biên dịch script, mở scene `Lobby`.
2. Có thể chọn menu `DienGame > Equipment > 01 - Kiểm tra assets trang bị`. Console phải báo đủ các sprite, không có asset missing.
3. Chọn menu `DienGame > Equipment > 02 - Cộng 2000 vàng và 50 mảnh (PLAYTEST)` để có tài nguyên kiểm thử trong PlayerPrefs local. Đây chỉ là nút dev, không cần cho gameplay thường.
4. Play scene `Lobby` -> mở `TRANG BỊ`. Trang đầu là `VŨ KHÍ / THÚ`, tab còn lại là `GIÁP / PHÙ ẤN` (hai món có sẵn được giữ nguyên).
5. Xạ thủ: chọn `Đạn Băng`, `Đạn Độc`, `Đạn Nổ` hoặc `Đạn Xuyên`; mua mở khóa rồi **TRANG BỊ**. Vào ải xem sprite đạn và hiệu ứng lên địch.
6. Sát thủ: chọn `Song Liềm`, `Liềm Xích`, `Trảo Độc`; xem thay đổi tầm/góc chém, sprite lóe vũ khí và hiệu ứng khi trúng địch.
7. Pháp sư: chọn `Hồ Ly Băng`, `Hỏa Điểu`, `Linh Tích Độc`, `Hư Không Linh`; vào trận thấy thú thay sprite và hiệu ứng đòn đánh thường. Thú Chimera mặc định giữ nguyên animation cũ.
8. Thử nâng từ +0 lên +1, thoát Play Mode, bật lại rồi kiểm tra cấp trang bị và item được chọn. Tiền vàng/mảnh phải bị trừ đúng chi phí.

## Luật cân bằng ban đầu (dùng cho playtest)

- Mỗi item chỉ có **một hướng chiến thuật**, không vừa có độc, làm chậm, nổ và xuyên cùng lúc.
- Nâng cấp từ **+0 đến +10** tăng **3,5% damage mỗi cấp**, không tăng độ mạnh hay thời lượng khống chế. Công thức chỉ số trang bị để hiển thị là `hệ số item × (1 + 0,035 × level)`.
- Xạ thủ: Đạn thường ×1,00; Băng ×0,92 và slow 22%/1,2 giây; Độc ×0,86 và DOT không cộng dồn; Nổ ×0,87 và splash 30% tối đa 2 địch phụ; Xuyên ×0,92 và địch thứ hai 60% damage. Đạn xuyên của **kỹ năng** vẫn dùng animation/cơ chế gốc, không bị đổi thành đạn trang bị.
- Sát thủ: Song Đao ×1,00; Song Liềm ×0,94 và tầm +12%; Liềm Xích ×0,83 và tầm +30%, góc 245°; Trảo Độc ×0,86, tầm ngắn 7% và DOT. Tất cả chỉ điều chỉnh chém thường, không phá kỹ năng đang có.
- Pháp sư: Chimera ×1,00; Hồ Ly Băng ×0,87; Hỏa Điểu ×0,83; Linh Tích Độc ×0,86; Hư Không Linh ×0,80. Các hệ số áp dụng lên đòn thường của thú, còn tăng cấp cường hóa ảnh hưởng stat Player như cơ chế nâng cấp chủ lực.
- Độc gây DOT dựa trên 22% sát thương đòn trúng chia trên 3 giây, bỏng 25% chia trên 2,5 giây. DOT **refresh không cộng dồn**. Boss giảm hiệu lực slow còn 10%/0,6 giây và không có stun.
- Các giá trị này là **baseline thử nghiệm**, chưa được chứng minh cân bằng bằng telemetry / kiểm thử trận thật.

## Ảnh & kiến trúc

Tất cả hình cần cho phiên bản hiện tại nằm trong `Assets/_Game/Resources/Equipment`:

- `Icons/`: icon từng item (xạ thủ / sát thủ / pháp sư / kiếm đấu sĩ).
- `Projectiles/`: 4 sprite đạn đặc biệt + 1 mẫu cơ bản.
- `Pets/`: 4 sprite thú ngoài Chimera gốc.
- `Statuses/`: 8 icon trạng thái.

Ảnh tham khảo layout nằm trong `Assets/_Game/Art/Equipment/Reference/`, **không được dùng làm UI tương tác**. Giao diện thực tế dựng từ `WorldHubPanels.cs`.

Logic chính:

- `Gameplay/Equipment/EquipmentCatalog.cs` — catalog 18 item, cân bằng.
- `Gameplay/Equipment/EquipmentInventory.cs` — chọn, mua mở khóa, nâng cấp và PlayerPrefs.
- `Gameplay/Equipment/EnemyEquipmentStatus.cs` — trạng thái chậm/độc/bỏng, icon báo trạng thái, sát thương lan.
- `Gameplay/World/WorldEquipmentStatApplier.cs` — áp dụng chỉ số khi vào ải (giữ giáp/phù ấn cũ).
- `Gameplay/Combat/Projectile.cs` — thay sprite/effect đạn xạ thủ, tương thích projectile pool và kỹ năng xuyên chuyên biệt.
- `Gameplay/Skills/Assassin/AssassinMeleeAttack.cs`, `AssassinCrescentVfx.cs` — các kiểu chém và sprite vũ khí khi đánh.
- `Gameplay/Skills/Mage/ChimeraCompanion.cs` — thay sprite thú và hiệu ứng đánh thường; vẫn giữ AI/leash, joystick thứ hai và collider.
- `Gameplay/Enemy/EnemyMovement.cs` — xử lý slow theo thời gian, không ghi đè tốc độ gốc.
- `Client/WorldHubPanels.cs` — UI thật: item, mở khóa, trang bị, cường hóa, vàng/mảnh.
- `Client/LobbySessionService.cs` — thêm enum dự phòng `Warrior=3` để có metadata kiếm đấu sĩ, **không mở class chơi được**.

## Giới hạn v1 / chưa được xác minh

- Project gốc **chưa có gameplay scene/controller Đấu sĩ** nên bốn sprite kiếm và definitions mới chỉ chuẩn bị trước, chưa sử dụng được trong trận.
- Bốn thú mới **chỉ có 1 sprite tĩnh + bob/flip + VFX kỹ năng cũ**. Chưa có atlas walk/attack 8 hướng mới cho từng thú; đổi thú vẫn có tác dụng combat thật.
- Trang bị mới thay sprite đạn và visual chém; vũ khí sát thủ chưa được rig/bake lại lên **từng frame chạy 8 hướng** của nhân vật gốc.
- Hiện là **save offline bằng PlayerPrefs**, không thể chống sửa client/cheat. Nếu sau này có backend/multiplayer, giao dịch và thống kê sát thương phải được server xác thực.
- Môi trường đóng gói này **không có Unity Editor/Play Mode**, nên mới kiểm tra cấu trúc file, sprite PNG, đường dẫn và code bằng kiểm tra tĩnh; **chưa thể xác nhận không có lỗi Console ở máy của bạn**.
- Bản FULL giữ nguyên các thư mục `Assets/Assets/...` và các file sửa lỗi cũ có sẵn trong `Assets(10).zip`. Hãy sao lưu trước khi copy và **không chạy installer cũ** vì có thể thay đổi các file khác ngoài tính năng trang bị.
