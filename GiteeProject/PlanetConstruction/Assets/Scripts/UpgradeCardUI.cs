using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UpgradeCardUI : MonoBehaviour
{
    public Text titleText;
    public Text descText;
    private UpgradeCard card;
    private UnityAction<UpgradeCard> callback;

    public void Setup(UpgradeCard cardData, UnityAction<UpgradeCard> onClick)
    {
        card = cardData;
        callback = onClick;
        titleText.text = card.title;
        descText.text = card.desc;
    }

    public void OnClick()
    {
        if (callback != null)
        {
            callback.Invoke(card);
        }
    }
}