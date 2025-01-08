namespace Nebula.Shared.Services;

[ServiceRegister]
public class PopupMessageService
{
    public Action<object>? OnPopupRequired;
    public Action<object>? OnCloseRequired;
    public void Popup(object obj)
    {
        OnPopupRequired?.Invoke(obj);
    }
    public void ClosePopup(object obj)
    {
        OnCloseRequired?.Invoke(obj);
    }
}