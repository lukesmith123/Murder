using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace MH
{

using MatchMap = System.Collections.Generic.Dictionary<string, MatchBone>;
using ConvertMap = System.Collections.Generic.Dictionary<string, string>; //<from path, to path>

public class MatchBone
{
    public string path;
    public bool found;
    public MatchBone(string p, bool found)
    {
        path = p;
        this.found = found;
    }
}

/// <summary>
/// used to make a new animation clip that mirror the joints 
/// </summary>
public class AnimMirrorEditorWindow : EditorWindow
{
	#region "configurable data"
    // configurable data

    #endregion "configurable data"

	#region "data"
    // data

    private static AnimMirrorEditorWindow ms_Instance;
    private AnimationClip m_Clip;
    private Transform m_AnimRoot; //the animator/animation gameobject

    private Axis m_SymBoneAxis = Axis.YZ;
    private Axis m_NonSymBoneAxis = Axis.YZ;

    private MatchMap m_MatchMap;
    private List<Regex> m_REs;
    private List<string> m_ReplaceStrs;

    #endregion "data"

	#region "unity event handlers"
    // unity event handlers

    [MenuItem("Window/Skele/Anim Mirror Tool")]
    public static void OpenWindow()
    {
        ms_Instance = (AnimMirrorEditorWindow)GetWindow(typeof(AnimMirrorEditorWindow));
        ms_Instance.title = "Animation Mirror Tool";
    }

    void OnEnable()
    {
        m_MatchMap = new MatchMap();
    }

    void OnInspectorUpdate()
    {
    }

    private Vector2 m_scrollPos = Vector2.zero;
    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        m_AnimRoot = EditorGUILayout.ObjectField("AnimatorGO", m_AnimRoot, typeof(Transform), true) as Transform;
        if( EditorGUI.EndChangeCheck() )
        {
            if( m_AnimRoot.GetComponent<Animator>() == null && 
                m_AnimRoot.GetComponent<Animation>() == null)
            {
                Dbg.LogWarn("The AnimatorGO must has Animation/Animator component!");
                m_AnimRoot = null;
            }
        }

        EditorGUI.BeginChangeCheck();
        m_Clip = EditorGUILayout.ObjectField("AnimClip", m_Clip, typeof(AnimationClip), false) as AnimationClip;
        if( EditorGUI.EndChangeCheck() )
        {
            _GetREs();
            _GenMatchMap();
        }

        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.MaxHeight(200f));
        {
            List<string> toDel = new List<string>();
            foreach( var entry in m_MatchMap )
            {
                string leftPath = entry.Key;
                string rightPath = entry.Value.path;
                bool bFound = entry.Value.found;

                GUILayout.BeginHorizontal();
                {
                    if(EUtil.Button("X", "Remove this entry", Color.red, GUILayout.Width(20)))
                    {
                        toDel.Add(leftPath);
                    }

                    string line;
                    if( bFound )
                    {
                        line = string.Format("{0} <-> {1}", leftPath, rightPath);
                    }
                    else
                    {
                        line = string.Format("{0} -> {1}", leftPath, rightPath);
                    }

                    GUILayout.Label(line);
                }
                GUILayout.EndHorizontal();
            }

            foreach( var d in toDel )
            {
                m_MatchMap.Remove(d);
            }
        }
        EditorGUILayout.EndScrollView();

        {
            m_SymBoneAxis = (Axis)EditorGUILayout.EnumPopup("Sym Bone Axis", m_SymBoneAxis, GUILayout.MaxWidth(200));
            m_NonSymBoneAxis = (Axis)EditorGUILayout.EnumPopup("Non-Sym Bone Axis", m_NonSymBoneAxis, GUILayout.MaxWidth(200));
        }

        EUtil.PushGUIEnable(m_AnimRoot != null && m_Clip != null);
        if( EUtil.Button("Apply Change!", Color.green))
        {
            string oriPath = AssetDatabase.GetAssetPath(m_Clip);
            string newPath = PathUtil.StripExtension(oriPath) + "_Mirror.anim";

            _CreateMirrorClip(newPath);
        }
        EUtil.PopGUIEnable();
    }

    void OnDestroy()
    {
    }

    void OnPlayModeChanged()
    {
        if (ms_Instance != null)
        {
            ms_Instance.Close();
        }
    }

    #endregion "unity event handlers"

	#region "public method"
    // public method

    #endregion "public method"

	#region "private method"

    private void _GenMatchMap()
    {
        EditorCurveBinding[] allBindings = AnimationUtility.GetCurveBindings(m_Clip);
        for( int idx = 0; idx < allBindings.Length; ++idx )
        {
            string thisPath = allBindings[idx].path;
            int matchREIdx = _IsMatchRE(thisPath);
            if (matchREIdx < 0)
            {
                continue;
            }
            else
            {
                --idx;
            }

            string thatPath = _GetMatchPath(thisPath, matchREIdx);
            bool bFound = _FindByPath(thatPath, allBindings);
            m_MatchMap[thisPath] = new MatchBone(thatPath, bFound);
            
             _ClearEntries(ref allBindings, thisPath, thatPath);
        }
    }

    private void _ClearEntries(ref EditorCurveBinding[] allBindings, string thisPath, string thatPath)
    {
        for(int idx = 0; idx < allBindings.Length; ++idx )
        {
            string p = allBindings[idx].path;
            if( p == thisPath || p == thatPath )
            {
                ArrayUtility.RemoveAt(ref allBindings, idx);
                --idx;
            }
        }
    }

    private bool _FindByPath(string thatPath, EditorCurveBinding[] allBindings)
    {
        foreach( var oneBinding in allBindings )
        {
            string onePath = oneBinding.path;
            if (thatPath == onePath)
                return true;
        }
        return false;
    }

    private string _GetMatchPath(string thisPath, int REidx)
    {
        Regex r = m_REs[REidx];
        string repl = m_ReplaceStrs[REidx];
        string newStr = r.Replace(thisPath, repl);
        return newStr;
    }

    private void _GetREs()
    {
        var res = AssetDatabase.LoadAssetAtPath(RELstPath, typeof(MirrorNameRegex)) as MirrorNameRegex;
        if( res == null )
        {
            res = new MirrorNameRegex();
            var lst = res.m_REPrLst = new List<MirrorNameRegex.REPair>();
            lst.Add(new MirrorNameRegex.REPair("Left", "Right"));
            lst.Add(new MirrorNameRegex.REPair("_L", "_R"));
            lst.Add(new MirrorNameRegex.REPair("\\.L", ".R"));
            lst.Add(new MirrorNameRegex.REPair("Right", "Left"));
            lst.Add(new MirrorNameRegex.REPair("_R", "_L"));
            lst.Add(new MirrorNameRegex.REPair("\\.R", ".L")); 
            AssetDatabase.CreateAsset(res, RELstPath);
        }

        m_REs = new List<Regex>();
        m_ReplaceStrs = new List<string>();
        foreach (var REPair in res.m_REPrLst)
        {
            m_REs.Add(new Regex(REPair.fromBoneRE));
            m_ReplaceStrs.Add(REPair.replaceString);
        }
    }

    private int _IsMatchRE(string p)
    {
        for(int idx = 0; idx < m_REs.Count; ++idx)
        {
            Regex r = m_REs[idx];
            if( r.IsMatch(p) )
            {
                return idx;
            }
        }

        return -1;
    }

    private void _CreateMirrorClip(string newPath)
    {
        ConvertMap convMap = new ConvertMap();
        foreach (var entry in m_MatchMap)
        {
            string fromPath = entry.Key;
            string toPath = entry.Value.path;
            bool bFound = entry.Value.found;

            convMap[fromPath] = toPath;
            if( bFound )
            {
                convMap[toPath] = fromPath;
            }
        }

        AnimationClip newClip = new AnimationClip();

        var allBindings = AnimationUtility.GetCurveBindings(m_Clip);

        foreach (var oneBinding in allBindings)
        {
            string bindingPath = oneBinding.path;
            string bindingProp = oneBinding.propertyName;
            bool bIsSymBone = false;

            // fix bindingPath
            AnimationCurve oldCurve = AnimationUtility.GetEditorCurve(m_Clip, oneBinding);
            EditorCurveBinding newBinding;
            if (convMap.ContainsKey(bindingPath))
            {
                string toPath = convMap[bindingPath];
                newBinding = EditorCurveBinding.FloatCurve(toPath, typeof(Transform), bindingProp);
                bIsSymBone = true;
            }
            else
            {
                newBinding = oneBinding;
            }

            Axis axisValue = bIsSymBone ? m_SymBoneAxis : m_NonSymBoneAxis;
            AnimationCurve newCurve = oldCurve; //default : newCurve = oldCurve

            // fix rotation curve and bindingProp
            switch(axisValue)
            {
                case Axis.XZ:
                    {
                        if( bindingProp == "m_LocalRotation.x")
                        { // x ==> -y
                            newCurve = _NegateCurve(oldCurve);
                            newBinding.propertyName = "m_LocalRotation.y";
                        }
                        else if( bindingProp == "m_LocalRotation.y")
                        { // y ==> -x
                            newCurve = _NegateCurve(oldCurve);
                            newBinding.propertyName = "m_LocalRotation.x";
                        }
                        else if (bindingProp == "m_LocalRotation.z")
                        { // z ==> w
                            newBinding.propertyName = "m_LocalRotation.w";
                        }
                        else if (bindingProp == "m_LocalRotation.w")
                        { // w ==> z
                            newBinding.propertyName = "m_LocalRotation.z";
                        }
                    }
                    break;
                case Axis.XY:
                    {
                        if(bindingProp == "m_LocalRotation.x")
                        { // x ==> z
                            newBinding.propertyName = "m_LocalRotation.z";
                        }
                        else if (bindingProp == "m_LocalRotation.y")
                        { // y ==> w
                            newBinding.propertyName = "m_LocalRotation.w";
                        }
                        else if (bindingProp == "m_LocalRotation.z")
                        { // z ==> x
                            newBinding.propertyName = "m_LocalRotation.x";
                        }
                        else if (bindingProp == "m_LocalRotation.w")
                        { // w ==> y
                            newBinding.propertyName = "m_LocalRotation.y";
                        }
                    }
                    break;
                case Axis.YZ:
                    {
                        // fix the keys
                        if (bindingProp == "m_LocalRotation.y" ||
                            bindingProp == "m_LocalRotation.z"
                            )
                        {
                            newCurve = _NegateCurve(oldCurve);
                        }

                    }
                    break;
                default:
                    Dbg.LogErr("AnimMirrorEditorWindow._CreateMirrorClip: unexpected mirror axis value: {0}", axisValue);
                    break;
            }

            // fix position curve
            axisValue = _GetAxisValueForPositionCurve(bindingPath);
            switch(axisValue)
            {
                case Axis.XZ:
                    {
                        if( bindingProp == "m_LocalPosition.y")
                        {
                            newCurve = _NegateCurve(oldCurve);
                        }
                    }
                    break;
                case Axis.XY:
                    {
                        if( bindingProp == "m_LocalPosition.z")
                        {
                            newCurve = _NegateCurve(oldCurve);
                        }
                    }
                    break;
                case Axis.YZ:
                    {
                        if( bindingProp == "m_LocalPosition.x" )
                        {
                            newCurve = _NegateCurve(oldCurve);
                        }
                    }
                    break;
                default:
                    Dbg.LogErr("AnimMirrorEditorWindow._CreateMirrorClip: unexpected mirror axis value (2nd): {0}", axisValue);
                    break;
            }

            // finally, set curve
            AnimationUtility.SetEditorCurve(newClip, newBinding, newCurve);
            
        } //end of foreach

        // finishing part
        var oldAnimType = (ModelImporterAnimationType)RCall.CallMtd("UnityEditor.AnimationUtility", "GetAnimationType", null, m_Clip);
        AnimationUtility.SetAnimationType(newClip, oldAnimType);

        AnimationClipSettings oldSettings = AnimationUtility.GetAnimationClipSettings(m_Clip);
        RCall.CallMtd("UnityEditor.AnimationUtility", "SetAnimationClipSettings", null, newClip, oldSettings);

        newClip.EnsureQuaternionContinuity();

        AssetDatabase.CreateAsset(newClip, newPath);
        AssetDatabase.Refresh();

        Dbg.Log("Created mirror-ed animation clip at: {0}", newPath);
    }

    /// <summary>
    /// get the axis value for position curve
    /// </summary>
    private Axis _GetAxisValueForPositionCurve(string bindingPath)
    {
        Transform curBone = m_AnimRoot.Find(bindingPath);
        if( curBone == null )
        {
            Dbg.LogErr("AnimMirrorEditorWindow._GetAxisValueForPositionCurve: failed to find transform named: {0}, Did you specify wrong AnimRoot?", bindingPath);
            return Axis.XZ;    
        }

        if( curBone == m_AnimRoot )
        {
            return m_NonSymBoneAxis; //if bindingPath points to AnimRoot, then just return non-sym bone axis, should be OK?
        }

        Transform parentBone = curBone.parent;

        string trPath = AnimationUtility.CalculateTransformPath(parentBone, m_AnimRoot);
        int REIdx = _IsMatchRE(trPath);
        if( REIdx < 0 )
        { //parent is non-sym bone
            return m_NonSymBoneAxis;
        }
        else
        {
            return m_SymBoneAxis;
        }
    }

    private static AnimationCurve _NegateCurve(AnimationCurve oldCurve)
    {
        var keys = oldCurve.keys;
        //Dbg.Log("{0}|{1}, keys cnt: {2}", bindingPath, bindingProp, oldCurve.length);
        for (int keyIdx = 0; keyIdx < keys.Length; ++keyIdx)
        {
            keys[keyIdx].value = -keys[keyIdx].value;
        }
        AnimationCurve theCurve = new AnimationCurve(keys);
        return theCurve;
    }

    #endregion "private method"

	#region "constant data"
    // constant data

    public const string RELstPath = "Assets/Skele/Res/BoneMirrorRELst.asset";

    public enum Axis 
    {
        XZ,
        XY,
        YZ,
    }

    #endregion "constant data"
}


}
