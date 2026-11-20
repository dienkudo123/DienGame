# DienGame – Ice Arena Collision V3

## Lỗi đã sửa
Bản trước chỉ có BoxCollider2D nhỏ ở chân props và 5 vùng collider trên ảnh nền, khiến Player/Enemy có thể đi xuyên qua các mảng vách đá, tinh thể, tường cổ nhìn thấy trong ảnh.

Bản này thay toàn bộ **IceArenaPremiumRuntime.cs** bằng mã hoàn chỉnh:
- 9 PolygonCollider2D ôm các vùng địa hình đặc được vẽ sẵn trong ảnh nền 941 × 1672.
- 16+ PolygonCollider2D theo đường bao alpha đã được tính sẵn của 19 sprite vật thể thực tế. Không còn chỉ collider ở chân.
- Tọa độ collider quy đổi bằng đúng Sprite/Transform nền, nên không lệch khi đổi PPU hoặc scale map.
- Vật thể có MapObstacle, isTrigger=false; Player được bật continuous collision detection và tự chuyển tới điểm trống gần nhất nếu xuất hiện trong vật cản.
- Không thay đổi AI/joystick/tốc độ di chuyển/skill/UI hay các đảo 1–4.

## Cài đặt
1. **Đóng Unity** và sao lưu project.
2. Giải nén ZIP vào **thư mục gốc Unity** và cho ghi đè đúng `Assets/_Game/Scripts/Gameplay/World/IceArenaPremiumRuntime.cs`.
3. Mở Unity > Lobby > Đảo 5 > Vào ải. Không chạy lại installer.
4. Thử đi vào vách đá bên trái, tinh thể bên phải, tường ở dưới: Player phải bị chặn.
5. Khi cần kiểm tra đường bao, chọn từng game object `SolidPainted_*` dưới `DienGame_IceArenaPremium_Stage05` ở Hierarchy, Scene view sẽ vẽ PolygonCollider2D.

## Lưu ý
- Đây là collision tĩnh cho địa hình Đảo 5; các khối hiệu ứng không có vật thể thì vẫn đi được.
- Ở trò chơi 2D top-down, toàn bộ vách đá được coi là không thể leo lên bằng đi bộ.
- Code và hình đã được so khớp theo asset nền. Chưa chạy Play Mode trong Unity tại môi trường này, nên hãy kiểm tra Console nếu project đang có hotfix khác.
