using UnityEngine;
using System.Collections;

public enum AgentNotificationType {
  Sex,
  Birth,
  Death
}

public class AgentNotifier : MonoBehaviour {

  public GameObject notifierPlanePrefab;
  public float notificationDuration = 1F;

  public Texture2D sexTexture;
  public Texture2D birthTexture;
  public Texture2D deathTexture;

  GameObject _notifierPlane;

  void Awake() {

  }

  public void Notify(AgentNotificationType type) {

    if (_notifierPlane == null) {
      Quaternion rotation = Quaternion.identity;
      rotation.eulerAngles = new Vector3(0, 90, 0);
      _notifierPlane = Instantiate(notifierPlanePrefab, transform.position, rotation) as GameObject;
      _notifierPlane.transform.parent = transform;
    }

    _notifierPlane.transform.localScale = Vector3.zero;
    Vector3 destination = transform.position + new Vector3(0, 4, 0);

    Material newMaterial = new Material(_notifierPlane.renderer.material);
    if (type == AgentNotificationType.Sex) {
      newMaterial.mainTexture = sexTexture;
    }
    else if (type == AgentNotificationType.Birth) {
      newMaterial.mainTexture = birthTexture;
    }
    else if (type == AgentNotificationType.Death) {
      newMaterial.mainTexture = deathTexture;
    }
    _notifierPlane.renderer.material = newMaterial;

    renderer.enabled = true;

    iTween.ScaleTo(_notifierPlane, new Vector3(0.25F, 0.25F, 0.25F), notificationDuration);
    iTween.MoveTo(_notifierPlane, iTween.Hash("position", destination, "time", notificationDuration, "oncomplete", "NotifyDone"));

    StartCoroutine(NotifyDone());

  }

  IEnumerator NotifyDone() {
    yield return new WaitForSeconds(notificationDuration * 1.5F);

    iTween.ScaleTo(_notifierPlane, Vector3.zero, notificationDuration / 2);
    //renderer.enabled = false;

    StartCoroutine(HideObject());

  }

  IEnumerator HideObject() {
    yield return new WaitForSeconds(notificationDuration / 2F);

    _notifierPlane.renderer.enabled = false;
  }

}
