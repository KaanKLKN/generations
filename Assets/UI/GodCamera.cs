using UnityEngine;
using System.Collections;

/** 
 * Defines the main camera behavior.
 */
public class GodCamera : MonoBehaviour {
        
    public float minX = -1000;
    public float maxX = 1000;
    public float minZ = -1000;
    public float maxZ = 1000;
    public float minY = 4;
    public float maxY = 100;
    
    public float moveSensitivity = 50f;
    public float zoomSensitivity = 50f;
    public float rotateSensitivity = 50f;
    
    
    public Map map;
    public float gridPadding = 25;
    void Start() {
        if (map != null) {
            Bounds gridBounds = map.Bounds();
            
            minX = gridBounds.min.x - gridPadding;
            maxX = gridBounds.max.x + gridPadding;
            minZ = gridBounds.min.z - gridPadding;
            maxZ = gridBounds.max.z + gridPadding;
            
        }
    }
        
    public class Modifiers
    {
        public bool leftAlt;
        public bool leftControl;
        public bool leftShift;

        public bool checkModifiers()
        {
            return (!leftAlt ^ Input.GetKey(KeyCode.LeftAlt)) &&
                (!leftControl ^ Input.GetKey(KeyCode.LeftControl)) &&
                (!leftShift ^ Input.GetKey(KeyCode.LeftShift));
        }
    }

    void Update() {
        
        
        #if ((UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR)
        
            iOSGestureUpdate();
        
        #else
        
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime;
        
            float translateX = Input.GetAxis("Horizontal") * Time.deltaTime;
            float translateZ = Input.GetAxis("Vertical") * Time.deltaTime;
            
            Modifiers rotationModifiers = new Modifiers { leftShift = true };
            bool shouldRotate = rotationModifiers.checkModifiers();
            
            Modifiers zoomModifiers = new Modifiers { leftControl = true };
            bool shouldZoomWithMouse = zoomModifiers.checkModifiers();
            
            
            float zoomTranslate = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
        
            if ( shouldRotate || Input.GetMouseButton(1) ) {
                // If SHIFT is held down, we orbit the camera around what we are currently looking at.
                Rotate(mouseX);
                ZoomIn(mouseY * zoomSensitivity);
                
            }
            else if (shouldZoomWithMouse || (Input.GetMouseButton(0) && Input.GetMouseButton(1)) ) {
                Pan(mouseX, mouseY);
            }
            else {
                Pan(translateX, translateZ);
                ZoomIn(zoomTranslate * zoomSensitivity);
            }
        
        #endif
               
    }
    
    void LateUpdate () {	
		if (_followObject != null) {
    		iTween.MoveUpdate(gameObject, CameraPositionForWorldPoint(_followObject.transform.position), 1.5f);
		}
	}
	
    
    GameObject _followObject;
    public void Follow(GameObject gameObject) {
        _followObject = gameObject;
    }
    
    public void FocusOnWorldPoint(Vector3 worldPoint) {
        iTween.MoveTo(gameObject, CameraPositionForWorldPoint(worldPoint), 2.0f);
    }
    
    Vector3 CameraPositionForWorldPoint(Vector3 worldPoint) {
        Ray ray = RayFromCenterOfScreen();
		RaycastHit hit = RaycastHitFromCenterOfScreen();

        Vector3 reversePoint = new Ray(worldPoint, -ray.direction).GetPoint(hit.distance);
		return new Vector3(reversePoint.x, transform.position.y, reversePoint.z);
    }
    
    
    void Rotate(float amount){
		RaycastHit hit = RaycastHitFromCenterOfScreen();
		transform.RotateAround(hit.point, Vector3.up, amount * rotateSensitivity / Time.timeScale);
    }
    
    Ray RayFromCenterOfScreen() {
        float width = camera.pixelWidth;
        return Camera.main.ScreenPointToRay(new Vector3(width / 2, camera.pixelHeight / 2, 0));
    }
    
    RaycastHit RaycastHitFromCenterOfScreen() {
        Ray ray = RayFromCenterOfScreen();
		RaycastHit hit;
		int layerMask = (1 << 8);
		Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
		return hit;
    }
    
    void ZoomIn(float amount) {
        float calculatedAmount = amount / Time.timeScale;
        Vector3 newPosition = transform.position + transform.forward * calculatedAmount;
        if (newPosition.y < minY || newPosition.y > maxY) {
            return;
        }
        transform.position = newPosition; 
    }
    
    void Pan(float panX, float panZ) {
        
        if (MayMoveXDistance(panX)) {
            transform.Translate(panX * moveSensitivity / Time.timeScale, 0, 0, Camera.main.transform);
        }
        
        if (MayMoveZDistance(panZ)) {
            transform.Translate(0, Mathf.Tan(Camera.main.transform.eulerAngles.x*(Mathf.PI/180)) * panZ * moveSensitivity / Time.timeScale, panZ * moveSensitivity / Time.timeScale, Camera.main.transform);
        }
    }
    
    bool MayMoveXDistance(float xDistance) {   
        RaycastHit hit = RaycastHitFromCenterOfScreen();
             
        Vector3 newXPosition = hit.point + new Vector3(xDistance, 0, 0);
        
        if (newXPosition.x < minX && // we are less than the allowed value
            xDistance < 0 // and we are trying to move even further in that direction
            ) {
            return false;
        }
        if (newXPosition.x > maxX && // we are less than the allowed value
            xDistance > 0 // and we are trying to move even further in that direction
            ) {
            return false;
        }
        return true;
    }
    
    bool MayMoveZDistance(float zDistance) {
        
        RaycastHit hit = RaycastHitFromCenterOfScreen();
        Vector3 newZPosition = hit.point + new Vector3(0, 0, zDistance);
        
        if (newZPosition.z < minZ && // we are less than the allowed value
            zDistance < 0 // and we are trying to move even further in that direction
            ) {
            return false;
        }
        if (newZPosition.z > maxZ && // we are less than the allowed value
            zDistance > 0 // and we are trying to move even further in that direction
            ) {
            return false;
        }
        return true;
    }

    
    Vector3 EstimatedLookAtPoint() {
        Vector3 lookAtPoint = transform.position + transform.forward * ApproximateGroundDistanceToLookAtPoint();
        return new Vector3(lookAtPoint.x, ground.transform.position.y, lookAtPoint.z);
    }
        
    public GameObject ground;
    float ApproximateGroundDistanceToLookAtPoint() {
        float cameraAngle = transform.eulerAngles.x;
        Debug.Log("cameraAngle: " + cameraAngle);
        float heightAboveGround = transform.position.y - ground.transform.position.y;
        return (float)((double)heightAboveGround / (double)Mathf.Tan(cameraAngle * Mathf.Deg2Rad));
    }
    
    float ActualGroundDistanceToLookAtPoint() {
        RaycastHit hit = RaycastHitFromCenterOfScreen();
        Vector3 groundPoint = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        
		return Vector3.Distance(hit.point, groundPoint);
    }
    


    // adjust accordingly in the inspector
    public float zoomNearLimit = 5;
    public float zoomFarLimit = 12;
    public float zoomScreenToWorldRatio = 3.0f;
    public float orbitScreenToWorldRatio = 1.0f;
    public float twistScreenToWorldRatio = 5.0f;

    // don't change these
    float distWeight;
    float zoomDistance;
    float lastf0f1Dist;
    
    // Silence some damn warnings on non-iOS platforms.
    #if (UNITY_IPHONE || UNITY_ANDROID)
        float zoomSpeed = 0;
        Vector3 orbitSpeed = Vector3.zero;
        float twistSpeed = 0;
    #endif
    
    void iOSGestureUpdate () {

        #if (UNITY_IPHONE || UNITY_ANDROID)

		// one finger gestures
		if (Input.touchCount == 1) {
		    
			
			// finger data
			Touch f0 = Input.GetTouch(0);
			
			// finger delta
			Vector3 f0Delta = new Vector3(-f0.deltaPosition.x, -f0.deltaPosition.y, 0);
			
			// if finger moving
			if (f0.phase == TouchPhase.Moved) {
				
				// compute orbit speed
				orbitSpeed += (f0Delta + f0Delta * distWeight) * orbitScreenToWorldRatio * Time.deltaTime;
			}
		}
		
		// two fingers gestures
		else if (Input.touchCount == 2) {
		    		    
			
			// fingers data
			Touch f0 = Input.GetTouch(0);
			Touch f1 = Input.GetTouch(1);
			
			// fingers positions
			Vector3 f0Pos = new Vector3(f0.position.x, f0.position.y, 0);
			Vector3 f1Pos = new Vector3(f1.position.x, f1.position.y, 0);
			
			// fingers movements
			Vector3 f0Delta = new Vector3(f0.deltaPosition.x, f0.deltaPosition.y, 0);
			Vector3 f1Delta = new Vector3(f1.deltaPosition.x, f1.deltaPosition.y, 0);
			
			// fingers distance
			float f0f1Dist = Vector3.Distance(f0.position, f1.position);
			
			// if both fingers moving
			if (f0.phase == TouchPhase.Moved && f1.phase == TouchPhase.Moved) {
				
				// fingers moving direction
				Vector3 f0Dir = f0Delta.normalized;
				Vector3 f1Dir = f1Delta.normalized;
				
				// dot product of directions
				float dot = Vector3.Dot(f0Dir, f1Dir);
				
				// if fingers moving in opposite directions
				if (dot < -0.7f) {
					
					float pinchDelta = f0f1Dist - lastf0f1Dist;
					
					// if fingers move more than a threshold
					if (Mathf.Abs(pinchDelta) > 2) {
												
						// if pinch out, zoom in 
						if (f0f1Dist > lastf0f1Dist && zoomDistance > zoomNearLimit) {
							zoomSpeed += (pinchDelta + pinchDelta * distWeight) * Time.deltaTime * zoomScreenToWorldRatio;
						}
						
						// if pinch in, zoom out
						else if (f0f1Dist < lastf0f1Dist && zoomDistance < zoomFarLimit) {
							zoomSpeed += (pinchDelta + pinchDelta * distWeight) * Time.deltaTime * zoomScreenToWorldRatio;
						}
					}
					
					// detect twist
					if (f0Delta.magnitude > 2 && f1Delta.magnitude > 2) {
						
						// homemade algorithm works, but needs code review
						Vector3 fingersDir = (f1Pos - f0Pos).normalized;
						Vector3 twistNormal = Vector3.Cross(fingersDir, Vector3.forward);
						Vector3 twistAxis = Vector3.Cross(fingersDir, twistNormal);
						float averageDelta = (f0Delta.magnitude + f1Delta.magnitude) / 2;
						if (Vector3.Dot(f0Dir, twistNormal) > 0.7f) {
							twistSpeed =  -twistAxis.z * averageDelta * Time.deltaTime * twistScreenToWorldRatio;
						}
						else if (Vector3.Dot(f0Dir, twistNormal) < -0.7f) {
							twistSpeed = twistAxis.z * averageDelta * Time.deltaTime * twistScreenToWorldRatio;
						}
					}
				}
			}
			
			// record last distance, for delta distances
			lastf0f1Dist = f0f1Dist;
			
			// decelerate zoom speed
			zoomSpeed = zoomSpeed * (1 - Time.deltaTime * 10);
		}
		
		// no touching, or too many touches (we don't care about)
		else {
			
/*          // bounce to zoom limits
            if (zoomDistance < zoomNearLimit) {
                zoomSpeed += (zoomDistance - zoomNearLimit) * zoomScreenToWorldRatio;
            }
            else if (zoomDistance > zoomFarLimit) {
                zoomSpeed += (zoomDistance - zoomFarLimit) * zoomScreenToWorldRatio;
            }
            
            // or decelerate
            else {
*/				zoomSpeed = zoomSpeed * (1 - Time.deltaTime * 10);
		//	}
		}
		
		// decelerate orbit speed
		orbitSpeed = orbitSpeed * (1 - Time.deltaTime * 5);

		// decelerate twist speed
		twistSpeed = twistSpeed * (1 - Time.deltaTime * 5);

		// apply zoom
        ZoomIn(zoomSpeed * Time.deltaTime);
		//transform.position += transform.forward * zoomSpeed * Time.deltaTime;

		zoomDistance = transform.position.magnitude;
		
		// apply orbit and twist
		Rotate(twistSpeed);
		Pan( orbitSpeed.x, orbitSpeed.y);
	//	transform.position = Vector3.zero;
	//	transform.localRotation *= Quaternion.Euler(orbitSpeed.y, orbitSpeed.x, twistSpeed);
	//	transform.position = -transform.forward * zoomDistance;
		
		// compensate for distance (ej. orbit slower when zoomed in; faster when out)
		distWeight = (zoomDistance - zoomNearLimit) / (zoomFarLimit - zoomNearLimit);
		distWeight = Mathf.Clamp01(distWeight);
		
		// Mark if we are currently mid-gesture.
		if (Mathf.Abs(orbitSpeed.x) > 0.01f || 
		    Mathf.Abs(orbitSpeed.y) > 0.01f || 
		    Mathf.Abs(orbitSpeed.z) > 0.01f || 
		    Mathf.Abs(zoomSpeed)    > 0.01f || 
		    Mathf.Abs(twistSpeed)   > 0.01f     ) {
	        if (gameGUI != null) {
    		    gameGUI.currentlyPerformingTouchGesture = true;
            }
		}
		else {
	        if (gameGUI != null) {
    		    gameGUI.currentlyPerformingTouchGesture = false;
            }
		}
    	
    	#endif
        
    }

    void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(EstimatedLookAtPoint(), 0.15f);
        
        RaycastHit hit = RaycastHitFromCenterOfScreen();
		Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hit.point, 0.15f);
    }

}

