DIENGAME - FIX LV5 VFX COLOR CONTRAST
====================================
Trieu Hoi Su Co Sinh: 6 cap Lv5 da duoc doi mau de KHAC RO voi cap 1-4.

Doi mau:
- TinhChamPhanLiet: Lv1-4 xanh bang -> Lv5 vang kim
- TraoLietHopKim: Lv1-4 do hong -> Lv5 xanh ngoc (xanh sinh the)
- PhaoSinhThe: Lv1-4 tim hong -> Lv5 xanh luc dien
- SongTrungOanhKich: Lv1-4 xanh luc -> Lv5 tim dien
- CongHuongSanMoi: Lv1-4 cam lua -> Lv5 do mau
- ManChanSongThe: Lv1-4 trang vang -> Lv5 xanh sapphire
- TrongTruongDiGioi: Lv1-4 tim -> Lv5 ho den vang den (giu nguyen, KHONG trong goi)

CAI DAT:
1) SAO LUU folder Assets/_Game/Resources/Mage/Vfx/
2) Giai nen zip vao THU MUC GOC project Unity, ghi de 6 file *_Lv5.png
3) Unity tu import lai. Neu chua cap nhat, chon PNG va Reimport hoac vao menu:
   DienGame > Mage > 2 - Reimport 14 Normal + Lv5 VFX Atlases
4) Play Mode, nang skill len Lv5 (co the dung menu test Lv5 neu da cai ban code Lv5).

Giu nguyen:
- Toan bo 7 atlas Lv1-4
- Atlas TrongTruongDiGioi_Lv5 (ho den vang den)
- FPS, PPU, scale, AI Chimera, collision, damage va skill gameplay
- Alpha va bo cuc 4 cot x 2 hang, moi o 512 x 512, o cuoi trong suot

Chi cap nhat asset PNG, KHONG co C# moi, nen it nguy co gay loi compile.

Luu y:
Đây là sửa bảng màu trên các ảnh Lv5 đã có; không vẽ lại cơ chế animation hoặc thay đổi hình dáng hiệu ứng.
