DIENGAME - ICE ARENA V2 | Bản map ingame chất lượng cao, ĐẢO 5
==============================================================
Dành cho Unity project đang có hệ thống WorldStageRuntime / map 5 đảo.

BẢN VÁ CHỈ THÊM FILE MỚI, KHÔNG GHI ĐÈ LobbyUI, WorldStageRuntime,
DienGameWorldInstaller, không đụng các class hoặc skill hiện có.

CÁCH CÀI:
1. Sao lưu project.
2. Giải nén toàn bộ nội dung ZIP vào GỐC project Unity (cùng cấp Assets/).
3. Mở Unity, đợi import hết. Nếu Sprite bị báo không tìm thấy:
   DienGame > Ice Arena V2 > Reimport Premium Art.
4. Vào Lobby > chọn ĐẢO 5 (CUNG BĂNG VÂN THIÊN) > Play.
5. Không cần chạy lại Installer, không cần chỉnh Inspector.

NỘI DUNG:
- 1 nền ICE ARENA phong cách 2D Việt huyền ảo, vẽ liền mạch, tỉ lệ dọc.
- 19 PNG địa hình riêng alpha thật: đá tuyết, thạch anh băng, cột đá cổ,
  thành đổ, cổng cổ, gai băng, phù điêu v.v.
- IceArenaPremiumRuntime.cs: tự thay nền ở ĐẢO 5, bỏ đồ cũ,
  dựng vật cản có BoxCollider2D + MapObstacle.
- Giữ nguyên tường vật lý sẵn có; chỉ tắt sprite tường cũ.
- Điểm sinh Player có chỗ trống. Không thay Enemy/Player/Skill/LevelUp.
- DienGameIcePremiumImport.cs: import Sprite Single/Uncompressed/Max Size 4096.

CHÚ Ý:
- Đây là bản nâng cấp hình ảnh/map ingame của ĐẢO 5; ĐẢO 1-4 giữ nguyên.
- Nền 941x1672 là ảnh gốc tạo mới (không phải ảnh bị kéo giãn lên rồi tự nhận là sắc nét).
- Chưa chạy được Unity Editor trên project máy người dùng, cần test Play Mode.
- Vật thể nổi từ tấm atlas đều có collider; các vật thể lớn vẽ trực tiếp trong
  background được bổ sung 5 collider ẩn phù hợp. Hình ảnh trên nền không được
  chuyển hóa thành mô hình 3D/mesh tự động.

GỠ BẢN NÂNG CẤP:
Xóa Assets/_Game/Scripts/Gameplay/World/IceArenaPremiumRuntime.cs và
Assets/_Game/Resources/World/IcePremium (và meta đi kèm),
trả map Đảo 5 về WorldStageRuntime cũ.
