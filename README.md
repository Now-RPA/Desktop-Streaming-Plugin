# Desktop Streaming Plugin

DesktopStreaming is a custom user plugin for the Now RPA platform by ServiceNow. It adds powerful desktop streaming capabilities to your RPA projects, allowing you to capture and stream your desktop over HTTP.

## Description

This plugin provides a range of functionalities for desktop streaming, including screen capture, MJPEG streaming, resolution control, and FPS adjustment. It supports streaming with or without the mouse cursor and implements a simple authentication mechanism for secure access.



https://github.com/user-attachments/assets/90258375-f2a3-43ad-9b05-8e537d18205f



## Setup and Installation

Before you can use the DesktopStreaming plugin in your Now RPA project, you need to set up the user plugin folder:

1. Locate your RPA Desktop Design Studio automation project folder.
2. Create a new folder named `UserPlugins` in this directory.
3. Inside the `UserPlugins` folder, create another folder named "DesktopStreaming".
4. Place the plugin .dll files in this folder.

To add the plugin to your project:

1. In the RPA Desktop Design Studio, open your project.
2. In the Project Explorer pane, right-click **User Plugins** and select **Add User Plugin**.
3. In the Available User Plugins dialog box, select `DesktopStreaming`.
4. Click **OK**.

The plugin will now appear in the Toolbox pane, ready for use in your automation workflows.

## Best Practices

1. Use appropriate IP address and port for your streaming setup.
2. Set reasonable FPS based on your system's performance and network bandwidth.
3. Consider disabling cursor display for sensitive information.
4. Always stop the streaming session when finished to free up resources.
5. Secure the generated authentication key to prevent unauthorized access to the stream.

## Streaming Setup

The plugin supports various streaming configurations:

1. **IP Address**: 
   Example: `"127.0.0.1"` (streams to localhost)
   
2. **Port**: 
   Example: `8080` (streams on port 8080)
   
3. **FPS**: 
   Example: `30` (streams at 30 frames per second)
   
4. **Display Cursor**: 
   Example: `true` (includes mouse cursor in the stream)

## Important: Streaming Management

Before using any other actions, it's crucial to start a streaming session. The URL returned by `StartStreaming` is required to access the stream.

### Start Streaming

Initializes a new desktop streaming session.

![Start Streaming](https://github.com/user-attachments/assets/b22e6e95-a547-4648-a0b4-dcababbd120d)

- Inputs:
  - `ipAddress`: String representing the IP address to bind the server to
  - `port`: Integer representing the port number to use for the server
  - `fps` (optional): Integer representing frames per second (default is 60)
  - `displayCursor` (optional): Boolean indicating whether to include the mouse cursor (default is true)
- Output: String URL for accessing the stream

### Stop Streaming

Stops the desktop streaming session.

- Input: None
- Output: None

**Best Practice**: Always stop your streaming session explicitly using `StopStreaming` to ensure proper resource management.

## Security Considerations

The plugin implements a simple authentication mechanism. When you start streaming, it generates a unique authentication key that must be included in the URL to access the stream. Ensure this key is securely managed and transmitted.

## Performance Considerations

Streaming at high resolutions or frame rates can be CPU-intensive. Adjust the FPS and resolution according to your system's capabilities and network bandwidth.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Disclaimer

This software is provided as-is. Use at your own risk. The authors are not responsible for any data loss or privacy issues that may occur from using this software.
