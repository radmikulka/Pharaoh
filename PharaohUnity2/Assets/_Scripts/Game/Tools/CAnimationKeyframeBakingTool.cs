// =========================================
// AUTHOR: Wojciech Drzymała
// DATE:   29.01.2026
// =========================================
// This is just a keyframe baking utility.
// It's not intended to go into the game!
// =========================================

#if UNITY_EDITOR
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CAnimationKeyframeBakingTool : MonoBehaviour
{
    [Tooltip("Animation Baker has different modes:\n\n" + 
             "- Animation: Plays an already existing animation and captures all objects' Transforms. " +
             "Useful for example when baking Splines into pure Transforms.\n\n" +
             "- Physics: Ideal for recording Physics into an animation. It will preferably start Baking " +
             "when GameObject is Enabled and continue for a given amount of time or until explicitly Stopped.")]
    [SerializeField] private BakingMode _bakingMode;
    
    // MODE: ANIMATION
    [ShowIf("_bakingMode", BakingMode.Animation)]
    [Header("Animation References")]
    [Tooltip("Reference to the Animation component which will start along with the start of the Baking process.")]
    [SerializeField] private Animation _originalAnimation;
    [ShowIf("_bakingMode", BakingMode.Animation)]
    [Tooltip("The Animation Clip the baker will play and recreate into a new Animation Clip file.")]
    [SerializeField] private AnimationClip _clipToRecreate;
    
    // MODE: PHYSICS
    [ShowIf("_bakingMode", BakingMode.Physics)]
    [Header("Animation Settings")]
    [Tooltip("If Enabled, the baking process will start automatically in Play Mode. Useful when physics simulation " +
             "starts immediately in Play Mode, which would result in skipping the beginning if prompted manually.")]
    [SerializeField] private bool _runBakingOnEnable;
    [ShowIf("_bakingMode", BakingMode.Physics)]
    [SerializeField] private float _maxBakingTime = 30f;
    [ShowIf("_bakingMode", BakingMode.Physics)]
    [Tooltip("This should be a point of reference for where the Animation component will be placed. Legacy animations " +
             "use hard references and all animated objects are referenced from one single point of origin.")]
    [SerializeField] private Transform _animatorOrigin;

    // OTHER SETTINGS
    [ShowIf("_isBakingModeChosen")]
    [Header("Baking Settings")]
    [Tooltip("In Seconds: How often the Animation Baker will take snapshots of the objects' position.")]
    [SerializeField] private float _keyframeInterval = 0.25f;
    [ShowIf("_isBakingModeChosen")]
    [SerializeField] private bool _bakePosition = true;
    [ShowIf("_isBakingModeChosen")]
    [SerializeField] private bool _bakeRotation = true;
    [ShowIf("_isBakingModeChosen")]
    [SerializeField] private bool _bakeScale;
    
    [ShowIf("_isBakingModeChosen")]
    [Header("Optimizing Animation")]
    [Tooltip("If Enabled, it will reduce the amount of keyframes by removing those that repeat previous values. " +
             "Good for diminishing the size of the final animation file.")]
    [SerializeField] private bool _removeRedundantKeyframes = true;
    [ShowIf("_isBakingModeChosen")]
    [Tooltip("If Enabled, this feature may help with problematic rotation keyframes. Since legacy Animations use " +
             "EulerAngles instead of Quaternion, sometimes when going above 360 degrees, EulerAngles may behave " +
             "unexpectedly, so this may help to smoothen those problems.")]
    [SerializeField] private bool _unwrapRotationCurves;

    [Tooltip("If Enabled, all keyframes in the Animation Clip will be set to Auto Smoothing.")]
    [ShowIf("_isBakingModeChosen")]
    [SerializeField] private bool _autoSmoothKeyframes = true;

    [ShowIf("_isBakingModeChosen")]
    [Header("Objects To Track")]
    [Tooltip("Insert references to all objects whose transforms you want to track during the baking process.")]
    [SerializeField] private GameObject[] _objectsToTrack;
    
    private Coroutine _bakingCoroutine;
    private AnimationClip _bakingClip;
    private float _currentTime;
    private int _recordedKeyframes;
    private bool _isBaking;
    
    private bool _isBakingModeChosen => _bakingMode != BakingMode.None;
    private enum BakingMode
    {
        None,
        Animation,
        Physics
    }
    
    [ShowIf("_isBakingModeChosen")]
    [Button("Start Baking")]
    public void StartBaking()
    {
        if (!Application.isPlaying)
            return;

        if (_isBaking)
        {
            Debug.LogWarning(" Baking Process is already running!");
            return;
        }
        
        if (_bakingMode == BakingMode.None)
        {
            Debug.LogWarning(" Baking Impossible! No Baking Mode was selected!");
            return;
        }
        
        if (_objectsToTrack == null || _objectsToTrack.Length < 1)
        {
            Debug.LogWarning(" Baking Impossible! No objects to follow!");
            return;
        }

        if (_bakingMode == BakingMode.Animation)
        {
            if (_originalAnimation == null)
            {
                Debug.LogWarning(" Baking Impossible! No originalAnimation assigned, which is required in Animation mode!");
                return;
            }

            if (_clipToRecreate == null)
            {
                Debug.LogWarning(" Baking Impossible! No clipToRecreate assigned, which is required in Animation mode!");
                return;
            }
        }

        if (_bakingMode == BakingMode.Physics)
        {
            if (_maxBakingTime <= 0.1f)
            {
                Debug.LogWarning(" Baking Impossible! Max baking time cannot be zero or negative!");
                return;
            }

            if (_animatorOrigin == null)
            {
                Debug.LogWarning(" Baking Impossible! No Animator Origin assigned!");
                return;
            }
        }

        if (_keyframeInterval <= 0.001f)
        {
            Debug.LogWarning(" Baking Impossible! Keyframe interval cannot be zero or negative!");
            return;
        }

        if (!_bakePosition && !_bakeRotation && !_bakeScale)
        {
            Debug.LogWarning(" Baking Impossible! You need to select at least one Transform property to track.");
            Debug.LogWarning(" (Position / Rotation / Scale)");
            return;
        }
        
        _isBaking = true;
        _currentTime = 0;
        _recordedKeyframes = 0;

        _bakingClip = new AnimationClip
        {
            frameRate = 60f,
            legacy = true
        };

        if (_bakingCoroutine != null)
            StopCoroutine(_bakingCoroutine);

        _bakingCoroutine = StartCoroutine(BakingCoroutine());
    }
    
    [ShowIf("_isBakingModeChosen")]
    [Button("Stop Baking")]
    private void StopBaking()
    {
        if (!_isBaking)
        {
            Debug.LogWarning(" [BAKING] Can't Stop Baking because there is no current Baking Process running!");
            return;
        }
        
        if (!Application.isPlaying)
            return;
        
        if(_bakingCoroutine != null)
            StopCoroutine(_bakingCoroutine);
        
        Debug.Log(" [BAKING] Finished baking! Saving animation file.");
        
        if (_removeRedundantKeyframes)
            RemoveRedundantKeys(_bakingClip);
        
        if (_unwrapRotationCurves)
            UnwrapRotationCurves(_bakingClip);

        if (_autoSmoothKeyframes)
            AutoSmoothKeyframes(_bakingClip);
        
        _bakingClip.EnsureQuaternionContinuity();
        
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Baked Animation",
            "BakedAnimation",
            "anim",
            "Save baked animation clip"
        );
        
        AssetDatabase.CreateAsset(_bakingClip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($" [BAKING] Baking finished! Animation clip saved at: {path}");
        _isBaking = false;
    }

    private void OnEnable()
    {
        if (_bakingMode == BakingMode.Physics && _runBakingOnEnable)
            StartBaking();
    }


    private IEnumerator BakingCoroutine()
    {
        var wait = new WaitForSeconds(_keyframeInterval);
        int maxKeyframes = 0;

        if (_bakingMode == BakingMode.Animation)
        {
            _originalAnimation.clip = _clipToRecreate;
            _originalAnimation.Stop();
            _originalAnimation.Rewind();
            _originalAnimation.Play();

            yield return null;
            
            maxKeyframes = Mathf.CeilToInt(_clipToRecreate.length / _keyframeInterval);
            while (_recordedKeyframes <= maxKeyframes)
            {
                if (_recordedKeyframes == 0)
                    yield return null;
                
                BakingTick();

                _recordedKeyframes++;
                int perc = (int) ((_recordedKeyframes / (float) maxKeyframes) * 100f);
                Debug.Log($" [BAKING] In Progress... Keyframes: {_recordedKeyframes} out of {maxKeyframes}... Progress {perc}%");
            
                _currentTime += _keyframeInterval;
                yield return wait;
            }
        }
        
        else if (_bakingMode == BakingMode.Physics)
        {
            maxKeyframes = (int)(_maxBakingTime / _keyframeInterval);
            while (_currentTime <= _maxBakingTime)
            {
                BakingTick();
                
                _recordedKeyframes++;
                int perc = (int) ((_recordedKeyframes / (float) maxKeyframes) * 100f);
                Debug.Log($" [BAKING] In Progress... Keyframes: {_recordedKeyframes} out of {maxKeyframes}... Progress {perc}%");
                
                _currentTime += _keyframeInterval;
                yield return wait;
            }
        }
            
        StopBaking();
        _bakingCoroutine = null;
    }

    private void BakingTick()
    {
        foreach (var obj in _objectsToTrack)
        {
            if (!obj) continue;
            RecordKeyframe(obj.transform);
        }
    }

    private void RecordKeyframe(Transform t)
    {
        string path = "";
        
        if (_bakingMode == BakingMode.Animation)
            path = AnimationUtility.CalculateTransformPath(t, _originalAnimation.transform);
        
        else if (_bakingMode == BakingMode.Physics)
            path = AnimationUtility.CalculateTransformPath(t, _animatorOrigin);
        
        if (_bakePosition)
        {
            AddKeyframe(path, "m_LocalPosition.x", t.localPosition.x);
            AddKeyframe(path, "m_LocalPosition.y", t.localPosition.y);
            AddKeyframe(path, "m_LocalPosition.z", t.localPosition.z);
        }

        if (_bakeRotation)
        {
            Quaternion q = t.localRotation;
            AddKeyframe(path, "m_LocalRotation.x", q.x);
            AddKeyframe(path, "m_LocalRotation.y", q.y);
            AddKeyframe(path, "m_LocalRotation.z", q.z);
            AddKeyframe(path, "m_LocalRotation.w", q.w);
        }

        if (_bakeScale)
        {
            AddKeyframe(path, "m_LocalScale.x", t.localScale.x);
            AddKeyframe(path, "m_LocalScale.y", t.localScale.y);
            AddKeyframe(path, "m_LocalScale.z", t.localScale.z);
        }
    }

    private void AddKeyframe(string path, string property, float value)
    {
        EditorCurveBinding binding = new EditorCurveBinding
        {
            path = path,
            type = typeof(Transform),
            propertyName = property
        };
        
        AnimationCurve curve = AnimationUtility.GetEditorCurve(_bakingClip, binding);
        if (curve == null)
            curve = new AnimationCurve();

        curve.AddKey(_currentTime, value);
        AnimationUtility.SetEditorCurve(_bakingClip, binding, curve);
    }
    
    private void AutoSmoothKeyframes(AnimationClip clip)
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var binding in bindings)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.length == 0)
                continue;

            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }

            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
    }

    
    private void RemoveRedundantKeys(AnimationClip clip)
    {
        // This method is AI-generated :D
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.keys.Length <= 1) continue;

            List<Keyframe> newKeys = new List<Keyframe>();
            newKeys.Add(curve.keys[0]);

            for (int i = 1; i < curve.keys.Length; i++)
            {
                if (!Mathf.Approximately(curve.keys[i].value, curve.keys[i - 1].value))
                    newKeys.Add(curve.keys[i]);
            }

            AnimationCurve optimizedCurve = new AnimationCurve(newKeys.ToArray());
            AnimationUtility.SetEditorCurve(clip, binding, optimizedCurve);
        }
    }
    
    private void UnwrapRotationCurves(AnimationClip clip)
    {
        // This method is AI-generated :D
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (!binding.propertyName.StartsWith("m_LocalRotation")) 
                continue;

            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.keys.Length <= 1) 
                continue;

            float lastValue = curve.keys[0].value;
            float accumulatedOffset = 0f;

            for (int i = 1; i < curve.keys.Length; i++)
            {
                float delta = curve.keys[i].value - lastValue;

                if (delta < -0.5f)
                    accumulatedOffset += 1f;
                else if (delta > 0.5f)
                    accumulatedOffset -= 1f;
                
                curve.keys[i].value += accumulatedOffset;

                lastValue = curve.keys[i].value;
            }

            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
    }
}
#endif