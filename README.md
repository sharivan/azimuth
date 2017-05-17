# Azimuth

A simple program to generate spherical model from an azimuthal map.

The program have two panels, the right panel where you load the azimuthal projection map and the left panel with the 3D visualization of the globe built across the map.

In the 3D panel, the commands are:

	- Mouse left click: Rotate the globe around its center.
	- Mouse right click: Zoom in/out.
	- Mouse wheel: Morphism between the globe and its azimutal projection.
	
The program have a toolbar containing the following items:

	- Open File: Where you load a map with azimuthal projection.
	- Selection: The normal behavior with mouse in the right panel, without any effect.
	- Pencil: You can draw in the right panel and see the result in the left panel.
	- Line: You can define a line in the right panel and see the result in the left panel.
	- Geodesic: You can draw a geodesic in the right panel and see the result in the left panel.
	- Define framework: You can define the framework of the azimuthal projection, moving or resizing the area of the projection. Use the left click with the mouse to drag the projection area and use the right click to resize the projection area in the right panel.
	- Change trace color: You can define the color used by tracing, applied to pencil, line and geodesic tracing.

This project depends on the module 3DTools. You can download it at:

https://github.com/sharivan/3DTools

Using the Visual Studio, add this project to your soluction and then add a reference to this project using the menu "Add>Reference..." before accessing the context menu by clicking at azimuth project in the Soluction Explorer.