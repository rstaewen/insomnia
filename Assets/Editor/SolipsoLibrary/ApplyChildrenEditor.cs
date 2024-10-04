using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(ApplyChildren), true)]
public class ApplyChildrenEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		ApplyChildren ac = (target as ApplyChildren);
		EditorGUILayout.Space();
		ac.applyAsActive = EditorGUILayout.Toggle("Activate When Applying", ac.applyAsActive);
		//EditorGUILayout.Space();
		ac.deactivateAfterApply = EditorGUILayout.Toggle("Deactivate After Applying",ac.deactivateAfterApply);
		//EditorGUILayout.Space();
		if(GUILayout.Button("Apply", GUILayout.Width(80f)))
		{

			GameObject[] toApply = ac.toApply;

			for(int i = 0; i< toApply.Length; i++)
			{
				if(ac.applyAsActive)
					toApply[i].SetActive(true);


				// Get currently selected object in "Hierarchy" view and store
				// its type, parent, and the parent's prefab origin
				GameObject selectedGameObject = toApply[i];
				PrefabType selectedPrefabType = PrefabUtility.GetPrefabType(selectedGameObject);
				GameObject parentGameObject = selectedGameObject;
				Object prefabParent = PrefabUtility.GetPrefabParent(selectedGameObject);
				
				// Notify the script this is modifying that something changed
				EditorUtility.SetDirty(target);
				
				// If the selected object is an instance of a prefab
				if (selectedPrefabType == PrefabType.PrefabInstance) {
					Debug.Log("applying prefab for object: "+toApply[i].name);
					// Replace parent's prefab origin with new parent as a prefab
					PrefabUtility.ReplacePrefab(parentGameObject, prefabParent,
					                            ReplacePrefabOptions.ConnectToPrefab);
				}


//				EditorUtility.SetDirty(toApply[i]);
//				if (PrefabUtility.GetPrefabType(toApply[i]) == PrefabType.PrefabInstance) {
//					PrefabUtility.ReplacePrefab(toApply[i], 
//					                            PrefabUtility.GetPrefabParent(toApply[i]), 
//					                            ReplacePrefabOptions.ConnectToPrefab);
//				}
				if(ac.deactivateAfterApply)
					toApply[i].SetActive(false);
			}
		}
	}
}
