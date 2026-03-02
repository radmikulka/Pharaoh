# Stylized Clouds Shader (Non-Volumetric)

This shader is designed to simulate stylized, non-volumetric clouds using vertex displacement. It is optimized for environments such as oceans, skies, or surreal landscapes. Below is a detailed explanation of each exposed parameter to help you achieve the visual result you're looking for.

## Material Parameters

🌥️ CloudHeight (float)
Defines the base height of the cloud layer. Useful to raise or lower the clouds in world or local space.

🎚️ Contact fade (float)
Controls how much the cloud fades when getting close to the ground or other geometry. A higher value creates a softer transition.

🎨 DarkColor (Color)
This is the shadow or darker part of the cloud. It adds depth and visual contrast.

☀️ LightColor (Color)
The main color of the clouds under light. This is usually the bright, sunlit part.

🌓 DarkPower (float)
Controls the strength of the darkness/shadow effect. Higher values increase contrast and volume perception.

🌬️ TurbulenceSpeed (float)
Controls how fast the cloud shapes move or evolve over time. Gives a sense of life and atmosphere.

🔍 Scale (float)
Determines the scale of the noise or texture pattern used to generate the cloud shapes. Lower values create larger, softer clouds; higher values add more detail.

## 📬 Support

For questions, feedback, or bug reports, please contact:  
📧 turishader@gmail.com

Thanks for using this shader! 🌤️
