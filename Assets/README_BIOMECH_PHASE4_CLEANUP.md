# DIENGAME — GIAI ĐOẠN 4: DỌN THÚ TRÙNG LẶP

## Kết quả

Chỉ còn **6 lựa chọn thú** trong trang TRANG BỊ của PHÁP SƯ:
- Chimera gốc (starter, giữ toàn bộ animation/kỹ năng cũ)
- Hồ Ly Băng Cơ Sinh
- Hỏa Điểu Cơ Sinh
- Độc Tích Cơ Sinh
- Hạch Hư Không Cơ Sinh
- Lang Cơ Sinh Tử Quang

**Đã xóa hẳn 4 món fantasy cũ khỏi EquipmentCatalog:** mage_icefox, mage_firehawk, mage_lizard, mage_void.
Đã xóa 8 PNG cũ kèm 8 .meta và thư mục Resources/Equipment/Pets rỗng (kèm .meta).
Không xóa bất cứ sprite Chimera gốc hoặc 5 thú mới nào.

## Tương thích với save hiện có

Nếu người chơi từng mở khóa/nâng cấp/trang bị một trong bốn thú fantasy cũ,
EquipmentInventory tự động chuyển quyền sở hữu, cấp cường hóa và lựa chọn đang dùng
sang thú biomech tương ứng miễn phí, chỉ chạy một lần. Không xóa các key save cũ
để có thể khôi phục phiên bản trước khi cần. Không chuyển chỉ số ngoài hệ thống
trang bị thú. Chimera/đạn/vũ khí và 3 ô trang bị khác giữ nguyên.

## Cài bản FULL

Giải nén bản FULL vào một project thử nghiệm hoặc thay Assets có sao lưu.
Bản FULL đã không còn những PNG cũ.

## Cài bản PATCH lên Phase 3

1. Sao lưu Unity project.
2. Giải nén PATCH vào thư mục gốc Unity (chỗ có Assets/); ghi đè các file C# tương ứng.
3. Khi Unity biên dịch lại, script Editor DienGameLegacyPetCleaner tự xóa đúng 8 PNG
   thú fantasy cũ và .meta của chúng. Đây là thao tác BỔ SUNG cho ZIP (giải nén
   zip riêng lẻ không thể xóa file cũ).
4. Nếu Unity không tự xóa, vào menu DienGame > Equipment > 04 - Xoa 4 thu fantasy trung lap.
5. Kiểm tra UI chọn Pháp Sư > TRANG BỊ: chỉ còn 6 lựa chọn, không còn bốn thú thuần fantasy.
6. Chạy menu DienGame > Equipment > 01 và 03 để kiểm tra icon, atlas; vào game test
   Chimera gốc và từng thú cơ–sinh.

## Giới hạn

Đã kiểm tra bằng script: danh sách catalog, các đường dẫn asset, không đổi
map/scene/VFX và mã gameplay khác. Không thể khẳng định đã biên dịch và chạy
Play Mode thực tế vì môi trường này không chạy Unity Editor.
