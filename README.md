# MeshAnimator Suite
### Currently, the package includes the following applications:
Smart Texture - This tool generates a material with a shader in your Unity project, which you can then apply to your avatar and animate. It facilitates the real-time transmission of images from your computer to VRChat.
Video Player - This application allows for the animation of video frames, enabling frame-by-frame video playback and integration into your project.



### Instructions (Smart Texture) ###

1. Set Image Resolution:
Adjust the "Height" and "Width" settings for the image that will be created.
2. Select Folder:
Choose the folder where the shader with the material will be created.
3. Create Shader:
Click the "Create Shader" button.
4.Assign Material to Object:
Add the created material to an object that has an empty slot in the Mesh Renderer's "Materials" section.
5. Choose Animation Folder:
Select a different folder, or keep the same one, where the animation folders (c0 and c1) will be created.
6. Assign Object with Material:
In the "GO w/ Smart Texture" slot, add the object to which you added the material.
7. Add FX Animator:
In the "FX Animator" slot, add your FX Animator.
8. Create Animation:
Click "Create Animation." After some time, everything will be created automatically, whether it takes a long or short time.

### FAQ (Smart Texture): ###
**Important!**
Before generating the texture, it is recommended to make a copy of your FX Animator.

- Why does the program take a long time to create a shader?
After entering the resolution, check the "Estimated Shader Compilation Time" section, which indicates the approximate time needed for shader compilation.
- Why does the program take a long time to create animations?
This is normal. Try deleting the old shader and material, then create a new shader with a lower resolution.
- What should I do if the shader or animations are not created?
Make sure you have launched the program correctly. If the image does not render or parts of it disappear over time, check if the animations that are constantly active (e.g., tail, ears, body, etc.) have WriteDefaults enabled.
- What should I do if the uploaded image is only partially visible?
Check the resolution of your image. It should not exceed the current shader resolution. For example, if your shader has a resolution of 32x32 pixels, ensure that the image in the "FolderForPicture" folder also has a resolution of 32x32 pixels.
- Where can I find images?
For example, you can use websites like "pixilart.com" to find images with a resolution of 32x32 pixels.
- Have additional questions?
Feel free to contact us on Discord.

### Instructions (Video Player) ###

