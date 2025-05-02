# FbxAnimUtilityKit-

**FbxAnimUtilityKit** is a collection of lightweight Unity Editor tools designed to streamline working with animation clips inside FBX files. It helps developers duplicate, migrate, and modify animations more efficiently during the asset import and setup process.

## ✨ Features

🔹 **Duplicate FBX Animations**  
Create independent `.asset` copies of embedded animation clips.

🔹 **Copy & Paste Animation Events**  
Easily transfer animation events between clips with the *Animation Events Copier* tool.

🔹 **Apply Avatar Masks**  
Batch-apply Avatar Masks to animation clips inside FBX models.

🔹 **User-Friendly UI**  
Drag-and-drop FBX workflow, batch operations, and validation feedback.

🔹 **Extensible Base Class**  
Easily add new FBX-related tools using the provided `BaseFbxTool` class.

---

## 📁 Usage

All tools can be accessed via Unity's top menu:

**`Pampero > Fbx and Animation Utility Kit`**

You can either:
- Open the **Main window** via:  
  `Pampero > Fbx and Animation Utility Kit > UtilityKitWindow`

- Or access each individual tool under:  
  `Pampero > Fbx and Animation Utility Kit > Tools`

---

## ✂️ Animation Events Migrator

This tool allows you to **copy** and **paste** animation events between different clips by using a virtual clipboard.

- 🔹 To **copy** events:  
  Select one or more `AnimationClip` assets, then click  
  `Pampero > Fbx and Animation Utility Kit > Tools > Copy Animation Events`

- 🔹 To **paste** events:  
  Select one or more target `AnimationClip` assets that match the names of the copied clips, then click  
  `Pampero > Fbx and Animation Utility Kit > Tools > Allocate Animation Events`

✅ Supports batch operations  
✅ Preserves all original animation events  
✅ Works directly in the Editor with no additional setup
