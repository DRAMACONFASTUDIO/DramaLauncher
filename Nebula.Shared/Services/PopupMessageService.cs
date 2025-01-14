namespace Nebula.Shared.Services;

[ServiceRegister]
public class PopupMessageService
{
    public Action<object>? OnCloseRequired;
    public Action<object>? OnPopupRequired;

    public void Popup(object obj)
    {
        OnPopupRequired?.Invoke(obj);
    }

    public void ClosePopup(object obj)
    {
        OnCloseRequired?.Invoke(obj);
    }
}