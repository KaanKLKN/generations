using UnityEngine;
using System.Collections;

public enum AgentNotificationType {
  Sex,
  Birth,
  Death,
  Ate
}

public class AgentNotifier : MonoBehaviour {

  public GameObject notifierPlanePrefab;
  public float notificationDuration = 1F;

  public Texture2D sexTexture;
  public Texture2D birthTexture;
  public Texture2D deathTexture;
  public Texture2D ateTexture;

  GameObject _notifierPlane;
  Material _myMaterial;

  void Awake() {

  }

  void Test() {
    Notify(AgentNotificationType.Sex);
    Debug.Log("Test");
  }

  bool inProgress = false;

  public void Notify(AgentNotificationType type) {

    if (inProgress)
      return;

    inProgress = true;

    if (_notifierPlane == null) {
      Quaternion rotation = Quaternion.identity;
      rotation.eulerAngles = new Vector3(0, 180, 0);
      _notifierPlane = Instantiate(notifierPlanePrefab, transform.position, rotation) as GameObject;
      _notifierPlane.transform.parent = transform;
      _myMaterial = new Material(_notifierPlane.renderer.material);
      _notifierPlane.renderer.material = _myMaterial;
    }

    _notifierPlane.transform.localScale = Vector3.zero;
    _notifierPlane.transform.position = transform.position;

    if (type == AgentNotificationType.Sex) {
      _myMaterial.mainTexture = sexTexture;
    }
    else if (type == AgentNotificationType.Birth) {
      _myMaterial.mainTexture = birthTexture;
    }
    else if (type == AgentNotificationType.Death) {
      _myMaterial.mainTexture = deathTexture;
    }
    else if (type == AgentNotificationType.Ate) {
      _myMaterial.mainTexture = ateTexture;
    }

    _notifierPlane.renderer.enabled = true;

    Vector3 destination = new Vector3(0, 2, 0);

    iTween.ScaleTo(_notifierPlane, new Vector3(4, 4, 4), notificationDuration);
    iTween.MoveTo(_notifierPlane, iTween.Hash("position", destination, "time", notificationDuration, "oncomplete", "NotifyDone", "islocal", true));

    StartCoroutine(NotifyDone());

  }

  IEnumerator NotifyDone() {
    yield return new WaitForSeconds(notificationDuration * 1.1F);

    iTween.ScaleTo(_notifierPlane, Vector3.zero, notificationDuration / 2);
    //renderer.enabled = false;

    StartCoroutine(HideObject());

  }

  IEnumerator HideObject() {
    yield return new WaitForSeconds(notificationDuration / 4F);

    _notifierPlane.renderer.enabled = false;
    inProgress = false;
  }

}
