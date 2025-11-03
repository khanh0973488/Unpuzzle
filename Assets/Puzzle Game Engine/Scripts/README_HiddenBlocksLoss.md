# Điều kiện thua mới: Block bị lock (ẩn)

## Mô tả
Tính năng này thêm một điều kiện thua mới vào game: **Nếu còn lượt di chuyển (moves) nhưng tất cả các block còn lại đều bị lock (ẩn), thì game sẽ thua**.

## Cách hoạt động

### 1. Kiểm tra điều kiện
- Hàm `HasRemainingMovableBlocksButAllHidden()` kiểm tra:
  - Có block nào có thể di chuyển (`canMove = true`) và đang active không?
  - Có block nào có thể di chuyển và không bị lock (`isLocked = false`) không?
  - Nếu có block có thể di chuyển nhưng tất cả đều bị lock → thua

### 2. Các nơi kích hoạt kiểm tra
- `LockBlock.cs`: Khi block bị lock/unlock thông qua `ReduceLockedCounter()` hoặc `UnlockByBomb()`
- `LevelManager.cs`: Khi hết lượt di chuyển

### 3. Hàm chính
```csharp
public void CheckForHiddenBlocksLossCondition()
```
- Có thể gọi từ bất kỳ đâu để kiểm tra điều kiện thua
- Tự động kiểm tra xem đã thắng chưa trước khi kiểm tra thua

## Cách sử dụng

### 1. Tự động
Tính năng hoạt động tự động khi:
- Block bị lock/unlock trong `LockBlock.ReduceLockedCounter()`
- Block bị unlock bằng bomb trong `LockBlock.UnlockByBomb()`
- Hết lượt di chuyển trong `LevelManager`

### 2. Thủ công
```csharp
LevelManager levelManager = FindObjectOfType<LevelManager>();
levelManager.CheckForHiddenBlocksLossCondition();
```

### 3. Demo
Sử dụng `HiddenBlocksLossDemo.cs`:
- Nhấn phím `H` để demo (sẽ lock một số block)
- Sử dụng `ShowBlockStatus()` để xem trạng thái các block

## Cấu hình

### LevelManager.cs
```csharp
// Thêm vào DecideOutcomeOnOutOfMoves()
if (HasRemainingMovableBlocksButAllHidden())
{
    ActivateLevelFailedPanel();
    return;
}
```

### LockBlock.cs
```csharp
// Thêm trong ReduceLockedCounter() sau khi giảm counter
LevelManager levelManager = GetComponentInParent<LevelManager>();
if (levelManager != null)
{
    levelManager.CheckForHiddenBlocksLossCondition();
}

// Thêm trong UnlockByBomb() sau khi unlock
LevelManager levelManager = GetComponentInParent<LevelManager>();
if (levelManager != null)
{
    levelManager.CheckForHiddenBlocksLossCondition();
}
```

## Lưu ý
- Điều kiện chỉ kích hoạt khi chưa thắng game
- Chỉ kiểm tra block có `canMove = true` và đang active
- Block bị "ẩn" ở đây có nghĩa là bị lock (`isLocked = true`) thay vì `SetActive(false)`
- Sử dụng `FindObjectsOfType<Block>()` để tìm tất cả block trong scene
- Có thể tùy chỉnh logic kiểm tra trong `HasRemainingMovableBlocksButAllHidden()`
- Block có thể bị lock thông qua `LockBlock` component với `isLocked = true`
