using System;

namespace RevolutionSolid
{
	/// <summary>
	/// Tool type.
	/// </summary>
	/// <remarks>
	/// You define the tool type when you add a tool to the generator. You can change
	/// an existing tool's type by calling <see cref="Generator.setToolType"/>.
	/// </remarks>
	/// <seealso cref="Generator.addTool"/>
	/// <seealso cref="Generator.setToolType"/>
	public enum ToolType
	{
		/// <summary>
		/// A subtractive tool removes material from the solid when it collides with it.
		/// </summary>
		Subtractive = 0,

		/// <summary>
		/// An additive tool adds material to the solid when it collides with it.
		/// </summary>
		Additive = 1,

		/// <summary>
		/// A mass preserving tool rearranges material when it collides with the solid. It
		/// removes material from the contact point and adds material to areas near the
		/// contact point. The solid's mass is preserved.
		/// </summary>
		MassPreserving = 2,

		/// <summary>
		/// A combination of multiple subtractive tools.
		/// </summary>
		MultiSubtractive = 3,

		/// <summary>
		/// A combination of multiple additive tools.
		/// </summary>
		MultiAdditive = 4,
	};

	/// <summary>
	/// Flags used to enable or disable the components displayed in the debug visualization.
	/// </summary>
	/// <remarks>
	/// Default value is ShowAll.
	/// </remarks>
	/// <seealso cref="Generator.setVisualizationFlags"/>
	[Flags]
	public enum VisualizationFlags : uint
	{
		/// <summary>Show the solid.</summary>
		ShowSolid = 1,

		/// <summary>Show the initially detected, discrete contour.</summary>
		ShowContour = 2,

		/// <summary>Show the detected contour after it has been smoothed.</summary>
		ShowSmoothContour = 4,

		/// <summary>Show the contour start point.</summary>
		ShowContourStart = 8,

		/// <summary>Show the calculated contour normals.</summary>
		ShowNormals = 16,

		/// <summary>Show the currently added tools.</summary>
		ShowTools = 32,

		/// <summary>The displayed contours pass through pixel centers instead of the top-left
		///   corners.</summary>
		OffsetContours = 64,

		/// <summary>Show everything (default).</summary>
		ShowAll = ShowSolid | ShowContour | ShowSmoothContour | ShowContourStart | ShowNormals | ShowTools | OffsetContours
	};

	/// <summary>
	/// Delegate to pass to the DLL for logging.
	/// </summary>
	/// <remarks>
	/// The logging callback is used to display messages useful for debugging.<br/>
	/// It will be called from the DLL when something goes wrong (for example, when an invalid
	///   argument was passed to a method).<br/>
	/// <strong>Important:</strong> The DLL keeps a reference to the delegate, so you should make
	///   sure that the delegate does not get collected by the GC while objects of the Generator
	///   class are still alive.<br/>
	/// <strong>Important:</strong> The delegate should use exception handling so that any
	///   exceptions thrown do not propagate into the native DLL's code.
	/// </remarks>
	/// <param name="message">The message to be displayed.</param>
	/// <seealso cref="Generator.setLoggingCallback"/>
	public delegate void LoggingCallback(string message);

	/// <summary>
	/// Class that generates solids of revolution.
	/// </summary>
	public class Generator
	{
		/// <summary>
		/// You can pass here the delegate that gets called by the DLL to print messages useful for
		///   debugging.
		/// </summary>
		/// <remarks>
		/// The delegate will be called when something goes wrong (for example, an invalid argument
		///   was passed to a method).<br/>
		/// You can pass null (or not call at all) to disable logging.<br/>
		/// It is recommended to have logging enabled during development and disabled in release build.
		/// </remarks>
		/// <param name="loggingCallback">Delegate.</param>
		/// <seealso cref="LoggingCallback"/>
		public static void setLoggingCallback(LoggingCallback loggingCallback)
		{
			NativeMethods.RevolutionSolid_setLoggingCallback(loggingCallback);
		}

		/// <summary>
		/// Creates an instance of the Generator class.
		/// </summary>
		/// <remarks>
		/// <strong>Important:</strong> You should call the <see cref="destroy"/> method when you no
		///   longer need the object (for example, on application quit) so that allocated memory
		///   will be released.
		/// </remarks>
		/// <seealso cref="destroy"/>
		public Generator()
		{
			m_handle = NativeMethods.RevolutionSolid_create();
		}

		/// <summary>
		/// Destroys the instance.
		/// </summary>
		/// <remarks>
		/// You should call this method when you no longer need the object (for example, on
		///   application quit) so that allocated memory will be released.<br/>
		/// It is safe to call this method multiple times. The first call destroys the instance and
		///   subsequent calls do nothing.<br/>
		/// <strong>Do not call</strong> any other methods of the class (apart from
		///   <see cref="destroy"/>) after you have called <see cref="destroy"/>.
		/// </remarks>
		public void destroy()
		{
			NativeMethods.RevolutionSolid_destroy(m_handle);

			m_handle = IntPtr.Zero;
		}

		/// <summary>
		/// Sets the template used as a starting point for generating the solid.
		/// </summary>
		/// <remarks>
		/// The template is a binary image. Black pixels correspond to "no material", while
		///   non-black pixels correspond to "material".<br/>
		/// Conceptually, the template is rotated about its Y axis to generate the solid.<br/>
		/// The template can be modified or "carved" by the user, using tools (<see cref="addTool"/>).<br/>
		/// The image file can be either one that was saved using the <see cref="saveAsTemplate"/>
		///   method, or an image file created by any painting application. Multiple image file
		///   formats are supported (for example *.bmp, *.jpg, *.png) but it is recommended to use
		///   the *.png format.<br/>
		/// Internally, the image is converted to binary if required.<br/>
		/// This method internally calls <see cref="restart"/> and <see cref="clearUndoStack"/>.
		/// </remarks>
		/// <param name="filename">Absolute or relative path to the image file to load as a
		///   template.</param>
		/// <seealso cref="getTemplate"/>
		/// <seealso cref="saveAsTemplate"/>
		public void setTemplate(string filename)
		{
			NativeMethods.RevolutionSolid_setTemplate(m_handle, filename);
		}

		/// <summary>
		/// Sets the template used as a starting point for generating the solid.
		/// </summary>
		/// <remarks>
		/// The template is a binary image. Black pixels correspond to "no material", while
		///   non-black pixels correspond to "material".<br/>
		/// Conceptually, the template is rotated about its Y axis to generate the solid.<br/>
		/// The template can be modified or "carved" by the user, using tools (<see cref="addTool"/>).<br/>
		/// The image file can be either one that was saved using the <see cref="saveAsTemplate"/>
		///   method, or an image file created by any painting application. Multiple image file
		///   formats are supported (for example *.bmp, *.jpg, *.png) but it is recommended to use
		///   the *.png format.<br/>
		/// Internally, the image is converted to binary if required.<br/>
		/// This method internally calls <see cref="restart"/> and <see cref="clearUndoStack"/>.
		/// </remarks>
		/// <param name="filename">Absolute or relative path to the image file to load as a
		///   template.</param>
		/// <param name="guardPixelX">X coordinate of the guard pixel.</param>
		/// <param name="guardPixelY">Y coordinate of the guard pixel.</param>
		/// <seealso cref="getTemplate"/>
		/// <seealso cref="saveAsTemplate"/>
		public void setTemplateEx(string filename, int guardPixelX, int guardPixelY)
		{
			NativeMethods.RevolutionSolid_setTemplateEx(m_handle, filename, guardPixelX, guardPixelY);
		}

		/// <summary>
		/// Returns the filename of the image that the generator is currently using as a template.
		/// </summary>
		/// <returns>The image filename, as set by the last call to <see cref="setTemplate"/>.</returns>
		/// <seealso cref="setTemplate"/>
		public string getTemplate()
		{
			return NativeMethods.RevolutionSolid_getTemplate(m_handle);
		}

		/// <summary>
		/// Save the current solid as a template.
		/// </summary>
		/// <remarks>
		/// You can later load the template using the <see cref="setTemplate"/> method.<br/>
		/// This method is mainly used during development to create different starting points for
		///   the user to select from.<br/>
		/// Additionally, you can use this method to partially save the user's progress and restore
		///   it later.<br/>
		/// Currently, only the solid's structure is saved. The generator's parameters, such as
		///   resolution, smoothing, etc. are not saved. If you want to preserve the generator's
		///   parameters you should store them manually in a separate file.<br/>
		/// The solid can be recreated exactly, if the generator's parameters match the ones that
		///   were used when the template was saved. If there is a parameter mismatch, the generated
		///   solid will be different.<br/>
		/// The template will be saved as an image file. Multiple image file formats are supported
		///   (for example *.bmp, *.jpg, *.png) but it is recommended to use the *.png format.
		/// </remarks>
		/// <param name="filename">Absolute or relative path for the image file where the template
		///   will be saved.</param>
		/// <seealso cref="setTemplate"/>
		public void saveAsTemplate(string filename)
		{
			NativeMethods.RevolutionSolid_saveAsTemplate(m_handle, filename);
		}

		/// <summary>
		/// Set the resolution in which the generator operates.
		/// </summary>
		/// <remarks>
		/// Higher resolution values generate meshes with more detail in the Y axis (more and smaller
		///   triangles), but require more memory and more CPU/GPU time to generate and render.<br/>
		/// Range: [10, 1000]<br/>
		/// Default value: 60<br/>
		/// This method internally calls <see cref="clearUndoStack"/>.
		/// </remarks>
		/// <param name="resolution">Resolution in pixels.</param>
		/// <seealso cref="getResolution"/>
		/// <seealso cref="setSlices"/>
		public void setResolution(int resolution)
		{
			NativeMethods.RevolutionSolid_setResolution(m_handle, resolution);
		}

		/// <summary>
		/// Returns the resolution value that the generator currently uses.
		/// </summary>
		/// <returns>The resolution value, in pixels.</returns>
		/// <seealso cref="setResolution"/>
		public int getResolution()
		{
			return NativeMethods.RevolutionSolid_getResolution(m_handle);
		}

		/// <summary>
		/// Set the number of slices in the XZ plane that the generated mesh will have.
		/// </summary>
		/// <remarks>
		/// The solid is created by rotating the template around its Y axis by 360 degrees. The
		///   number of slices controls into how many slices the 360 degrees range will be divided.
		///   The default value, 72, generates 72 slices, which corresponds to one slice every
		///   360/72=5 degrees.<br/>
		/// Higher values generate meshes with more detail in the XZ plane (more and smaller
		///   triangles) but require more memory and more CPU/GPU time to generate and render.<br/>
		/// Range: [3, 360]<br/>
		/// Default value: 72<br/>
		/// This method internally calls <see cref="clearUndoStack"/>.
		/// </remarks>
		/// <param name="slices">The number of slices.</param>
		/// <seealso cref="getSlices"/>
		/// <seealso cref="setResolution"/>
		public void setSlices(int slices)
		{
			NativeMethods.RevolutionSolid_setSlices(m_handle, slices);
		}

		/// <summary>
		/// Returns the number of slices that the generator currently uses.
		/// </summary>
		/// <returns>The number of slices.</returns>
		/// <seealso cref="setSlices"/>
		public int getSlices()
		{
			return NativeMethods.RevolutionSolid_getSlices(m_handle);
		}

		/// <summary>
		/// Enables or disables smoothing.
		/// </summary>
		/// <remarks>
		/// When smoothing is enabled, the generated mesh is smoother but may lose detail near edges.<br/>
		/// The amount of smoothing can be controlled by changing the kernel size and deviation parameters
		///   (<see cref="setSmoothingKernelSize"/> and <see cref="setSmoothingKernelDeviation"/>).<br/>
		/// Smoothing is enabled by default.<br/>
		/// This method internally calls <see cref="clearUndoStack"/>.
		/// </remarks>
		/// <param name="enable">Enable (true) or disable (false) smoothing.</param>
		/// <seealso cref="isSmoothingEnabled"/>
		/// <seealso cref="setSmoothingKernelSize"/>
		/// <seealso cref="setSmoothingKernelDeviation"/>
		public void enableSmoothing(bool enable = true)
		{
			NativeMethods.RevolutionSolid_enableSmoothing(m_handle, enable);
		}

		/// <summary>
		/// Returns true if smoothing is enabled.
		/// </summary>
		/// <returns>true if smoothing is enabled.</returns>
		/// <seealso cref="enableSmoothing"/>
		public bool isSmoothingEnabled()
		{
			return NativeMethods.RevolutionSolid_isSmoothingEnabled(m_handle);
		}

		/// <summary>
		/// Set the size of the Gaussian kernel that is used to smooth the generated mesh.
		/// </summary>
		/// <remarks>
		/// Higher values will produce a smoother mesh, but details such as sharp edges will be lost.<br/>
		/// Range: [3, 35], odd values only (i.e. 3, 5, 7, ... 35)<br/>
		/// Default value: 5<br/>
		/// This method internally calls <see cref="clearUndoStack"/>.
		/// </remarks>
		/// <param name="smoothingKernelSize">The Gaussian kernel size.</param>
		/// <seealso cref="getSmoothingKernelSize"/>
		public void setSmoothingKernelSize(int smoothingKernelSize)
		{
			NativeMethods.RevolutionSolid_setSmoothingKernelSize(m_handle, smoothingKernelSize);
		}

		/// <summary>
		/// Returns the size of the Gaussian kernel that the generator currently uses to smooth the mesh.
		/// </summary>
		/// <returns>The Gaussian kernel size.</returns>
		/// <seealso cref="setSmoothingKernelSize"/>
		public int getSmoothingKernelSize()
		{
			return NativeMethods.RevolutionSolid_getSmoothingKernelSize(m_handle);
		}

		/// <summary>
		/// Set the deviation (sigma) of the Gaussian kernel that is used to smooth the mesh.
		/// </summary>
		/// <remarks>
		/// Higher values will produce a smoother mesh, but details such as sharp edges will be lost.<br/>
		/// Range: [0, inf], if set to 0 then sqrt(kernelSize) is used.<br/>
		/// Default value: 0<br/>
		/// This method internally calls <see cref="clearUndoStack"/>.
		/// </remarks>
		/// <param name="smoothingKernelDeviation">The Gaussian kernel deviation.</param>
		/// <seealso cref="getSmoothingKernelDeviation"/>
		public void setSmoothingKernelDeviation(float smoothingKernelDeviation)
		{
			NativeMethods.RevolutionSolid_setSmoothingKernelDeviation(m_handle, smoothingKernelDeviation);
		}

		/// <summary>
		/// Returns the deviation (sigma) of the Gaussian kernel that the generator currently
		///   uses to smooth the mesh.
		/// </summary>
		/// <returns>The Gaussian kernel deviation.</returns>
		/// <seealso cref="setSmoothingKernelDeviation"/>
		public float getSmoothingKernelDeviation()
		{
			return NativeMethods.RevolutionSolid_getSmoothingKernelDeviation(m_handle);
		}

		/// <summary>
		/// Enable debug visualization.
		/// </summary>
		/// <remarks>
		/// Debug visualization is displayed in a pop-up window. The window displays the template
		///   image (i.e. one slice), before it is rotated to produce the final mesh. The displayed
		///   image includes any modifications made by the user using the tools.<br/>
		/// Visualization might provide some insight in case the generated mesh does not look like
		///   what you would expect.<br/>
		/// Visualization consumes a lot of CPU cycles, so it is advised to enable it only during
		///   development and only when a problem arises.<br/>
		/// Visualization is disabled by default.
		/// </remarks>
		/// <param name="enable">Enable (true) or disable (false) debug visualization.</param>
		/// <seealso cref="isVisualizationEnabled"/>
		/// <seealso cref="setVisualizationFlags"/>
		public void enableVisualization(bool enable = true)
		{
			NativeMethods.RevolutionSolid_enableVisualization(m_handle, enable);
		}

		/// <summary>
		/// Returns true if debug visualization is enabled.
		/// </summary>
		/// <returns>true if debug visualization is enabled.</returns>
		/// <seealso cref="enableVisualization"/>
		public bool isVisualizationEnabled()
		{
			return NativeMethods.RevolutionSolid_isVisualizationEnabled(m_handle);
		}

		/// <summary>
		/// Enables or disables the components that will be visible in the visualization window.
		/// </summary>
		/// <param name="visualizationFlags">Visualization flags.</param>
		/// <seealso cref="getVisualizationFlags"/>
		/// <seealso cref="VisualizationFlags"/>
		public void setVisualizationFlags(VisualizationFlags visualizationFlags)
		{
			NativeMethods.RevolutionSolid_setVisualizationFlags(m_handle, visualizationFlags);
		}

		/// <summary>
		/// Returns the flags currently used for visualization.
		/// </summary>
		/// <returns>The flags currently used for visualization.</returns>
		/// <seealso cref="setVisualizationFlags"/>
		/// <seealso cref="VisualizationFlags"/>
		public VisualizationFlags getVisualizationFlags()
		{
			return NativeMethods.RevolutionSolid_getVisualizationFlags(m_handle);
		}

		/// <summary>
		/// Returns the window handle of the visualization window.
		/// </summary>
		/// <returns>The native window handle (HWND) of the visualization window, cast as IntPtr.</returns>
		public IntPtr getVisualizationWindowHandle()
		{
			return NativeMethods.RevolutionSolid_getVisualizationWindowHandle(m_handle);
		}

		/// <summary>
		/// Add a tool which you will use to modify the solid.
		/// </summary>
		/// <remarks>
		/// Tools are used to modify the solid (add, remove or move material). Modification is executed
		///   when the tool's mesh collides with the solid's mesh. Collision detection requires
		///   the tool's localToWorldMatrix and the solid's worldToLocalMatrix. These two matrices
		///   should be regularly updated by calling <see cref="setToolLocalToWorldMatrix"/> and
		///   <see cref="setSolidWorldToLocalMatrix"/>.<br/>
		/// Currently, all single tools are approximated by a sphere. It is assumed that this sphere has a
		///   radius of 0.5 units and is centered at (0, 0, 0) in its local coordinate frame, which
		///   is the sphere that Unity produces for a sphere game object.<br/>
		/// You can change the sphere radius by applying uniform scale from within Unity and
		///   notifying the generator by calling the <see cref="setToolLocalToWorldMatrix"/>.<br/>
		/// If required, you can approximate shapes other than a sphere by creating multiple tools
		///   and grouping them together in Unity. This is what the multi-tool types do.<br/>
		/// The number of tools you can have is currently unlimited.
		/// </remarks>
		/// <param name="tool">
		/// User-defined, unique integer ID of the tool.<br/>
		/// This is used in subsequent methods to change this tool's parameters.<br/>
		/// If a tool already exists with this ID, it is replaced.</param>
		/// <param name="localToWorldMatrix">
		/// The 4x4 matrix that transforms the 3D model of the tool from the local coordinate frame
		///   to the world coordinate frame.<br/>
		/// The matrix is internally used to detect collision with the generated solid.<br/>
		/// The matrix can contain any combination of translation, rotation and scaling, but only
		///   uniform scaling is currently supported. If the matrix contains non-uniform scaling,
		///   collision detection will not work correctly.<br/>
		/// You can retrieve this matrix from Unity using
		///   GameObject.GetComponent&lt;Renderer&gt;().localToWorldMatrix.<br/>
		/// You should update the matrix whenever the tool's transformation changes in Unity by
		///   calling <see cref="setToolLocalToWorldMatrix"/>.<br/>
		/// If the matrix is not yet known when you add the tool, you can pass the identity matrix,
		///   but make sure that you eventually provide the correct matrix (by calling
		///   <see cref="setToolLocalToWorldMatrix"/>) before you call <see cref="generate"/>.</param>
		/// <param name="toolType">Tool type.</param>
		/// <param name="active">
		/// Whether the tool is initially active.<br/>
		/// An active tool modifies the mesh (adds, removes or moves material, depending on the tool
		///   type) if it collides with it. An inactive tool is not checked for collisions and does
		///   not modify the generated mesh.<br/>
		/// The main use of this property is to prevent accidental modification of the generated
		///   mesh. One method to achieve this would be to only activate a tool when the user
		///   presses a button in a controller and deactivate it when they release the button.</param>
		/// <seealso cref="setToolLocalToWorldMatrix"/>
		/// <seealso cref="setSolidWorldToLocalMatrix"/>
		public void addTool(int tool, UnityEngine.Matrix4x4 localToWorldMatrix, ToolType toolType, bool active)
		{
			NativeMethods.RevolutionSolid_addTool(m_handle, tool, ref localToWorldMatrix, toolType, active);
		}

		/// <summary>
		/// Add a multi-tool which you will use to modify the solid.
		/// </summary>
		/// <remarks>
		/// Tools are used to modify the solid (add, remove or move material). Modification is executed
		///   when the tool's mesh collides with the solid's mesh. Collision detection requires
		///   the tool's localToWorldMatrix and the solid's worldToLocalMatrix. These two matrices
		///   should be regularly updated by calling <see cref="setToolLocalToWorldMatrix"/> and
		///   <see cref="setSolidWorldToLocalMatrix"/>.<br/>
		/// While single-tool types are approximated by a single sphere, multi-tool types use
		///   multiple spheres to approximate the tool's shape. The relative position of these
		///   spheres should be provided when you add the tools to the generator. You can use the
		///   <see cref="Voxelizer"/> class to approximate a tool's triangle mesh by spheres and
		///   retrieve the positions of these spheres.
		/// The number of tools you can have is currently unlimited.
		/// </remarks>
		/// <param name="tool">
		/// User-defined, unique integer ID of the tool.<br/>
		/// This is used in subsequent methods to change this tool's parameters.<br/>
		/// If a tool already exists with this ID, it is replaced.</param>
		/// <param name="voxelCenters">A list of voxel centers (relative position of the multi-tool's
		///   spheres). You can generate this list from a tool model's triangle mesh using the
		///   <see cref="Voxelizer"/> class.
		/// </param>
		/// <param name="voxelSize">The voxel size value that was passed to the <see cref="Voxelizer"/>
		///   class. This is used to calculate the sphere radii.
		/// </param>
		/// <param name="localToWorldMatrix">
		/// The 4x4 matrix that transforms the 3D model of the tool from the local coordinate frame
		///   to the world coordinate frame.<br/>
		/// The matrix is internally used to detect collision with the generated solid.<br/>
		/// The matrix can contain any combination of translation, rotation and scaling, but only
		///   uniform scaling is currently supported. If the matrix contains non-uniform scaling,
		///   collision detection will not work correctly.<br/>
		/// You can retrieve this matrix from Unity using
		///   GameObject.GetComponent&lt;Renderer&gt;().localToWorldMatrix.<br/>
		/// You should update the matrix whenever the tool's transformation changes in Unity by
		///   calling <see cref="setToolLocalToWorldMatrix"/>.<br/>
		/// If the matrix is not yet known when you add the tool, you can pass the identity matrix,
		///   but make sure that you eventually provide the correct matrix (by calling
		///   <see cref="setToolLocalToWorldMatrix"/>) before you call <see cref="generate"/>.</param>
		/// <param name="toolType">Tool type.</param>
		/// <param name="active">
		/// Whether the tool is initially active.<br/>
		/// An active tool modifies the mesh (adds, removes or moves material, depending on the tool
		///   type) if it collides with it. An inactive tool is not checked for collisions and does
		///   not modify the generated mesh.<br/>
		/// The main use of this property is to prevent accidental modification of the generated
		///   mesh. One method to achieve this would be to only activate a tool when the user
		///   presses a button in a controller and deactivate it when they release the button.</param>
		/// <seealso cref="setToolLocalToWorldMatrix"/>
		/// <seealso cref="setSolidWorldToLocalMatrix"/>
		public void addMultiTool(int tool, UnityEngine.Vector3[] voxelCenters, float voxelSize, UnityEngine.Matrix4x4 localToWorldMatrix, ToolType toolType, bool active)
		{
			NativeMethods.RevolutionSolid_addMultiTool(m_handle, tool, voxelCenters, voxelCenters.Length, voxelSize, ref localToWorldMatrix, toolType, active);
		}

		/// <summary>
		/// Removes the tool with the given ID.
		/// </summary>
		/// <param name="tool">The ID of the tool that was created with the <see cref="addTool"/>
		///   method.</param>
		public void removeTool(int tool)
		{
			NativeMethods.RevolutionSolid_removeTool(m_handle, tool);
		}

		/// <summary>
		/// Removes all tools.
		/// </summary>
		/// <seealso cref="removeTool"/>
		public void removeAllTools()
		{
			NativeMethods.RevolutionSolid_removeAllTools(m_handle);
		}

		/// <summary>
		/// Activates or deactivates a tool.
		/// </summary>
		/// <remarks>
		/// An active tool modifies the mesh (adds, removes or moves material, depending on the tool type)
		///   if it collides with it. An inactive tool is not checked for collisions and does not
		///   modify the generated mesh.<br/>
		/// The main use of this property is to prevent accidental modification of the generated
		///   mesh. One method to achieve this would be to only activate a tool when the user
		///   presses a button in a controller and deactivate it when they release the button.<br/>
		/// </remarks>
		/// <param name="tool">The ID of the tool that was created with the <see cref="addTool"/>
		///   method.</param>
		/// <param name="active">Whether to activate (true) or deactivate (false) the tool.</param>
		/// <seealso cref="isToolActive"/>
		/// <seealso cref="deactivateAllTools"/>
		public void activateTool(int tool, bool active)
		{
			NativeMethods.RevolutionSolid_activateTool(m_handle, tool, active);
		}

		/// <summary>
		/// Deactivates all tools.
		/// </summary>
		/// <seealso cref="activateTool"/>
		public void deactivateAllTools()
		{
			NativeMethods.RevolutionSolid_deactivateAllTools(m_handle);
		}

		/// <summary>
		/// Returns true if the tool is currently active.
		/// </summary>
		/// <param name="tool">The ID of the tool that was created with the <see cref="addTool"/>
		///   method.</param>
		/// <returns>true if the tool is currently active, false if not.</returns>
		/// <seealso cref="activateTool"/>
		public bool isToolActive(int tool)
		{
			return NativeMethods.RevolutionSolid_isToolActive(m_handle, tool);
		}

		/// <summary>
		/// Changes the tool type.
		/// </summary>
		/// <param name="tool">The ID of the tool that was created with the <see cref="addTool"/>
		///   method.</param>
		/// <param name="toolType">The new tool type.</param>
		/// <seealso cref="getToolType"/>
		/// <seealso cref="addTool"/>
		public void setToolType(int tool, ToolType toolType)
		{
			NativeMethods.RevolutionSolid_setToolType(m_handle, tool, toolType);
		}

		/// <summary>
		/// Returns the tool type.
		/// </summary>
		/// <param name="tool">The ID of the tool that was created with the <see cref="addTool"/>
		///   method.</param>
		/// <returns>The tool type.</returns>
		/// <seealso cref="addTool"/>
		public ToolType getToolType(int tool)
		{
			return NativeMethods.RevolutionSolid_getToolType(m_handle, tool);
		}

		/// <summary>
		/// Set the 4x4 matrix that transforms the 3D model of the tool from the local coordinate
		///   frame to the world coordinate frame.
		/// </summary>
		/// <remarks>
		/// The matrix is internally used to detect collision with the generated solid.<br/>
		/// The matrix can contain any combination of translation, rotation and scaling, but only
		///   uniform scaling is currently supported. If the matrix contains non-uniform scaling,
		///   collision detection will not work correctly.<br/>
		/// You can retrieve this matrix from Unity using
		///   GameObject.GetComponent&lt;Renderer&gt;().localToWorldMatrix.<br/>
		/// You should call this method whenever the tool's transformation changes in Unity and
		///   before calling <see cref="generate"/>.<br/>
		/// Typically you call this method for all tools from MonoBehaviour's Update() method, then
		///   call <see cref="generate"/> to generate the solid's triangle mesh and finally
		///   <see cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out int[])"/>
		///   or <see cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out UnityEngine.Vector2[], out int[])"/>
		///   to retrieve the generated mesh.
		/// </remarks>
		/// <param name="tool">The ID of the tool that was created with the <see cref="addTool"/>
		///   method.</param>
		/// <param name="localToWorldMatrix">The 4x4 transformation matrix, typicaly acquired from
		///   Unity.</param>
		/// <seealso cref="getToolLocalToWorldMatrix"/>
		/// <seealso cref="generate"/>
		public void setToolLocalToWorldMatrix(int tool, UnityEngine.Matrix4x4 localToWorldMatrix)
		{
			NativeMethods.RevolutionSolid_setToolLocalToWorldMatrix(m_handle, tool, ref localToWorldMatrix);
		}

		/// <summary>
		/// Returns the last known localToWorldMatrix for this tool.
		/// </summary>
		/// <param name="tool">The ID of the tool that was created with the <see cref="addTool"/>
		///   method.</param>
		/// <returns>The 4x4 localToWorldMatrix.</returns>
		/// <seealso cref="setToolLocalToWorldMatrix"/>
		public UnityEngine.Matrix4x4 getToolLocalToWorldMatrix(int tool)
		{
			UnityEngine.Matrix4x4 localToWorldMatrix;

			NativeMethods.RevolutionSolid_getToolLocalToWorldMatrix(m_handle, tool, out localToWorldMatrix);

			return localToWorldMatrix;
		}

		/// <summary>
		/// Set the 4x4 matrix that transforms the 3D model of the generated solid from the world
		///   coordinate frame to the local coordinate frame.
		/// </summary>
		/// <remarks>
		/// The generated solid's bounding volume is initially centered at (0, 0, 0). Its height
		///   (size along the Y axis) is 1 unit and its length and depth (sizes along the X and Z
		///   axes, respectively) depend on the currently used template. This is the solid's local
		///   coordinate frame.<br/>
		/// You would probably apply transformations from within Unity to bring this to the world
		///   coordinate frame. You should call this method whenever you change this transformation.<br/>
		/// The matrix is internally used to detect collision with the tools.<br/>
		/// The matrix can contain any combination of translation, rotation and scaling, but only
		///   uniform scaling is currently supported. If the matrix contains non-uniform scaling,
		///   collision detection will not work correctly.<br/>
		/// You can retrieve this matrix from Unity using
		///   GameObject.GetComponent&lt;Renderer&gt;().worldToLocalMatrix.<br/>
		/// Typically you call this method from MonoBehaviour's Update() method, then call
		///   <see cref="generate"/> to generate the solid's triangle mesh and finally
		///   <see cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out int[])"/>
		///   or <see cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out UnityEngine.Vector2[], out int[])"/>
		///   to retrieve the generated mesh.
		/// </remarks>
		/// <param name="worldToLocalMatrix">The 4x4 transformation matrix, typicaly acquired from
		///   Unity.</param>
		/// <seealso cref="getSolidWorldToLocalMatrix"/>
		/// <seealso cref="generate"/>
		public void setSolidWorldToLocalMatrix(UnityEngine.Matrix4x4 worldToLocalMatrix)
		{
			NativeMethods.RevolutionSolid_setSolidWorldToLocalMatrix(m_handle, ref worldToLocalMatrix);
		}

		/// <summary>
		/// Returns the last known worldToLocalMatrix for the generated solid.
		/// </summary>
		/// <remarks>
		/// This is the matrix that was set with the last call to <see cref="setSolidWorldToLocalMatrix"/>.
		/// </remarks>
		/// <returns>The solid's 4x4 worldToLocalMatrix.</returns>
		/// <seealso cref="setSolidWorldToLocalMatrix"/>
		public UnityEngine.Matrix4x4 getSolidWorldToLocalMatrix()
		{
			UnityEngine.Matrix4x4 worldToLocalMatrix;

			NativeMethods.RevolutionSolid_getSolidWorldToLocalMatrix(m_handle, out worldToLocalMatrix);

			return worldToLocalMatrix;
		}

		/// <summary>
		/// Returns the maximum bounding volume of the solid.
		/// </summary>
		/// <remarks>
		/// The maximum bounding volume is used to scale vertex coordinates in order to produce
		///   texture coordinates by the provided tri-planar texturing shader.<br/>
		/// The maximum bounding volume of the solid depends solely on the dimensions of the
		///   template used. Thus, it only changes when you call <see cref="setTemplate"/> and you
		///   typically use this method to update the texturing shader's parameters after a call to
		///   <see cref="setTemplate"/>.<br/>
		/// For a detailed description of the solid's bounding volume, see
		///   <see cref="setSolidWorldToLocalMatrix"/>.
		/// </remarks>
		/// <param name="worldBoundsMin">Minimum coordinate value in all 3 axes.</param>
		/// <param name="worldBoundsMax">Maximum coordinate value in all 3 axes.</param>
		/// <seealso cref="setSolidWorldToLocalMatrix"/>
		public void getWorldBounds(out UnityEngine.Vector3 worldBoundsMin, out UnityEngine.Vector3 worldBoundsMax)
		{
			NativeMethods.RevolutionSolid_getWorldBounds(m_handle, out worldBoundsMin, out worldBoundsMax);
		}

		/// <summary>
		/// Pushes a snapshot of the modified template in the undo stack.
		/// </summary>
		/// <remarks>
		/// You can later return to this snapshot by calling <see cref="popUndo"/>.<br/>
		/// The <see cref="pushUndo"/> and <see cref="popUndo"/> methods work similarly to
		///   <see cref="saveAsTemplate"/> and <see cref="setTemplate"/>, but no files are used.
		///   Instead the modified template is stored internally in a stack.<br/>
		/// Similarly to <see cref="saveAsTemplate"/>, only the solid's structure is saved. The
		///   generator's parameters, such as resolution, smoothing, etc. are not saved. If these
		///   parameters are changed between calls to <see cref="pushUndo"/> and <see cref="popUndo"/>,
		///   the solid will not be recreated exactly.<br/>
		/// Currently, the undo stack size is unlimited.<br/>
		/// Note: Some methods will reset the undo stack. This will be indicated in the individual
		///   methods' documentation.
		/// </remarks>
		/// <seealso cref="popUndo"/>
		/// <seealso cref="clearUndoStack"/>
		public void pushUndo()
		{
			NativeMethods.RevolutionSolid_pushUndo(m_handle);
		}

		/// <summary>
		/// Pops the undo stack, restoring the modified template to the state that was saved by the
		///   last call to <see cref="pushUndo"/>.
		/// </summary>
		/// <remarks>
		/// If the undo stack is empty, the call has no effect.
		/// </remarks>
		/// <seealso cref="pushUndo"/>
		/// <seealso cref="clearUndoStack"/>
		public void popUndo()
		{
			NativeMethods.RevolutionSolid_popUndo(m_handle);
		}

		/// <summary>
		/// Clears the undo stack, deleting any saved snapshots.
		/// </summary>
		/// <seealso cref="pushUndo"/>
		/// <seealso cref="popUndo"/>
		public void clearUndoStack()
		{
			NativeMethods.RevolutionSolid_clearUndoStack(m_handle);
		}

		/// <summary>
		/// Restarts the generator.
		/// </summary>
		/// <remarks>
		/// This method restores the template to its original state. Any modifications made by the
		///   user through the use of tools will be lost.<br/>
		/// This method internally calls <see cref="clearUndoStack"/>.
		/// </remarks>
		public void restart()
		{
			NativeMethods.RevolutionSolid_restart(m_handle);
		}

		/// <summary>
		/// Generates the solid's triangle mesh.
		/// </summary>
		/// <remarks>
		/// Typically you call this method from your MonoBehaviour's Update() method. The typical
		///   pipeline is:
		/// <list type="number">
		/// <item>Call <see cref="setSolidWorldToLocalMatrix"/> if the solid's transformation has
		///   changed.</item>
		/// <item>Call <see cref="setToolLocalToWorldMatrix"/> for all tools whose transformations
		///   have changed.</item>
		/// <item>Call <see cref="generate"/>.</item>
		/// <item>If <see cref="generate"/> returned true, call
		///   <see cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out int[])"/>
		///   or <see cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out UnityEngine.Vector2[], out int[])"/>
		///   to retrieve the generated mesh.</item>
		/// <item>Update your renderer with the generated mesh.</item>
		/// </list>
		/// </remarks>
		/// <returns>true if the mesh has changed since the last call to <see cref="generate"/>. If
		///   this method returns false, the mesh has not changed, so you do not need to update
		///   your renderer.</returns>
		/// <seealso cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out int[])"/>
		/// <seealso cref="getMesh(out UnityEngine.Vector3[], out UnityEngine.Vector3[], out UnityEngine.Vector2[], out int[])"/>
		public bool generate()
		{
			return NativeMethods.RevolutionSolid_generate(m_handle);
		}

		/// <summary>
		/// Retrieve the generated mesh.
		/// </summary>
		/// <remarks>
		/// This should only be called after a call to <see cref="generate"/>.<br/>
		/// The lengths of the <paramref name="vertices"/> and <paramref name="normals"/> arrays are
		/// the same. The length of the <paramref name="indices"/> array is 3*<paramref name="vertices"/>.Length.
		/// Every 3 consecutive indices in the <paramref name="indices"/> array form a triangle.
		/// Thus, the generated mesh has <paramref name="indices"/>.Length/3 triangles.
		/// </remarks>
		/// <param name="vertices">Array of vertices.</param>
		/// <param name="normals">Array of unit length normal vectors (one for each vertex).</param>
		/// <param name="indices">Array of triangle indices (3 consecutive indices form a triangle).</param>
		/// <seealso cref="generate"/>
		public void getMesh(out UnityEngine.Vector3[] vertices, out UnityEngine.Vector3[] normals, out int[] indices)
		{
			int numVertices;
			int numTriangles;

			NativeMethods.RevolutionSolid_getMeshSize(m_handle, out numVertices, out numTriangles);

			vertices = new UnityEngine.Vector3[numVertices];
			normals = new UnityEngine.Vector3[numVertices];
			indices = new int[numTriangles * 3];

			NativeMethods.RevolutionSolid_getMesh(m_handle, vertices, normals, indices);
		}

		/// <summary>
		/// Retrieve the generated mesh.
		/// </summary>
		/// <remarks>
		/// This should only be called after a call to <see cref="generate"/>.<br/>
		/// The lengths of the <paramref name="vertices"/> and <paramref name="normals"/> arrays are
		/// the same. The length of the <paramref name="indices"/> array is 3*<paramref name="vertices"/>.Length.
		/// Every 3 consecutive indices in the <paramref name="indices"/> array form a triangle.
		/// Thus, the generated mesh has <paramref name="indices"/>.Length/3 triangles.
		/// </remarks>
		/// <param name="vertices">Array of vertices.</param>
		/// <param name="normals">Array of unit length normal vectors (one for each vertex).</param>
		/// <param name="texCoords">Array of texture coordinates (one for each vertex).</param>
		/// <param name="indices">Array of triangle indices (3 consecutive indices form a triangle).</param>
		/// <seealso cref="generate"/>
		public void getMesh(out UnityEngine.Vector3[] vertices, out UnityEngine.Vector3[] normals, out UnityEngine.Vector2[] texCoords, out int[] indices)
		{
			int numVertices;
			int numTriangles;

			NativeMethods.RevolutionSolid_getMeshSize(m_handle, out numVertices, out numTriangles);

			vertices = new UnityEngine.Vector3[numVertices];
			normals = new UnityEngine.Vector3[numVertices];
			texCoords = new UnityEngine.Vector2[numVertices];
			indices = new int[numTriangles * 3];

			NativeMethods.RevolutionSolid_getMeshEx(m_handle, vertices, normals, texCoords, indices);
		}

		IntPtr m_handle = IntPtr.Zero;
	}
}
