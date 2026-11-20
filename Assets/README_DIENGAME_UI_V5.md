# DienGame UI Revamp v5 — giao diện mobile đẹp và dễ đọc

**Nền sử dụng:** DienGame Phase 4 (đã dọn thú fantasy trùng). Bản FULL của gói này được tạo từ Phase 4. Bản PATCH chỉ sửa UI; không xóa hoặc sửa dữ liệu trang bị, kỹ năng, quái, map, sprite Chimera.

## Đã làm trong code

1. **Chọn nhân vật:** Thiết kế lại 3 card Xạ thủ / Sát thủ / Pháp sư bằng layout theo tỷ lệ màn hình, icon, trạng thái chọn; nút Xác nhận và Quay lại giữ đúng sự kiện.
2. **Sảnh:** Khung chọn đảo, thanh thông tin, nút Vào ải, navigation, top bar và nhãn địa danh dùng theme thống nhất. Không thay các đảo, map hoặc chức năng.
3. **Trang bị:** UI chọn trang bị theo class, cuộn danh sách, mở khóa, trang bị, nâng cấp, giáp/phù ấn; giữ lưu PlayerPrefs và chi phí cũ. Danh sách có cả 6 thú hợp lệ của Pháp sư.
4. **Nhiệm vụ:** Trạng thái số đảo hoàn thành, tiến độ hạ quái, phần thưởng và nút nhận thưởng kết nối `WorldInventory.TryClaimKillQuest` như cũ.
5. **Cửa hàng:** Thẻ vật phẩm có hình minh họa, nút mua với giá thật; giữ `BuyFragments` và `BuyChest` như cũ.
6. **Lên cấp:** Ba card kỹ năng có icon, tiêu đề, mô tả và touch target lớn; giữ nguyên `Show(options,onSelected)` và luồng pause/upgrade.
7. **HUD:** Giữ nguyên khung HP/EXP hình tròn của người dùng, cải thiện font/contrast, thanh Boss và popup kết quả; vòng joystick thú có texture mới nhưng không đổi vector di chuyển.
8. **Theme Unity 9-slice:** `Assets/_Game/Resources/UITheme`, 14 PNG có `.meta` và viền 9-slice. Màu xanh đêm, vàng ấm, xanh ngọc.
9. **Tiếng Việt:** Các label dùng auto sizing, xuống dòng và font TMP dynamic. Editor tự tạo `Resources/UITheme/VietnameseFont.asset` từ `Assets/TextMesh Pro/Fonts/LiberationSans.ttf` đã có trong project; không cần chép font thủ công. Menu cứu hộ: `DienGame > UI > 01 - Kiem tra font tieng Viet`.

## Cài đặt

### Project đang ở Phase 4
1. Sao lưu project trước khi nâng cấp.
2. Giải nén `DienGame_UI_V5_PATCH.zip` vào **thư mục gốc** chứa `Assets/`, `Packages/`, `ProjectSettings/`.
3. Unity biên dịch/import. Chờ Console hết thông báo, rồi chạy scene Lobby.
4. Chạy `DienGame > UI > 01 - Kiem tra font tieng Viet` nếu chữ có ô vuông hoặc thiếu dấu.

### Tạo project từ bản FULL
Giải nén `DienGame_UI_V5_FULL.zip`, lấy thư mục `Assets/` vào project Unity dùng cùng Unity version/packages hiện tại. Gói FULL không chứa `ProjectSettings`/`Packages`; không phải Unity project độc lập.

## Kiểm tra Play Mode quan trọng
- Lobby: top bar, navigation, chọn đảo, nhận điểm danh; xem text có tràn không.
- Bấm vào profile: chọn một trong 3 class, xác nhận, quay lại; kiểm tra màn portrait và chữ tiếng Việt.
- Trang bị: cuộn danh sách → mở khóa → trang bị → nâng cấp, kiểm tra vàng/mảnh sau thao tác.
- Nhiệm vụ: bấm nhận thưởng khi đủ 20 kills; kiểm tra nhận đúng 1 lần.
- Cửa hàng: mua mảnh và rương; kiểm tra kho/tiền vàng.
- Vào trận với ba class: chọn kỹ năng khi lên cấp, boss HP, popup kết quả, joystick thú, HUD HP/EXP.
- Kiểm tra ở tỷ lệ 9:16 và 19.5:9, độ phân giải khoảng 360x800 và 1080x1920.

## Tính tương thích / giới hạn
- Không có engine Unity Editor trong môi trường xuất gói; mã mới đã qua kiểm tra cấu trúc và dữ liệu, nhưng **chưa xác nhận compile và Play Mode trên Unity thật**.
- Hình `DienGame_UI_Revamp_Preview.png` là bản xem trước phong cách (render mẫu), không phải screenshot Unity đã chạy.
- Font dynamic TMP phụ thuộc bộ font đã có trong Project. Nếu module TMP đang báo lỗi shader hoặc fallback, chạy công cụ font ở menu Editor nói trên.
- Trạng thái và chi phí dùng từ game gốc, không chỉnh dữ liệu save.
