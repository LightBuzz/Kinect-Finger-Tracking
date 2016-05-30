# Kinect Finger Tracking
The most accurate way to track fingers using Kinect v2. Why is it the best solution out there?
* Tracks both hands simultaneously.
* It lets you access the contour of the hand and the positions of the fingertips.
* You can get the coordinates in the Depth, Color, and Camera space (3D and 2D).
* Works between 0.5 and 5 meters (1.6 to 16 feet).
* It is blazingly fast!

## Video
[Watch on YouTube](https://youtu.be/YH_yiaxUm7k)

![Finger tracking with Kinect - Vangos Pterneas](http://pterneas.com/wp-content/uploads/2016/01/kinect-finger-tracking.png)

## Usage

Finger Tracking is under the LightBuzz.Vitruvius.FingerTracking namespace. This namespace should be imported whenever you need to use the figner tracking capabilities.

    using LightBuzz.Vitruvius.FingerTracking;

Everything is encapsulated into the HandsController class. To use the HandsController class, first create a new instace:

    private HandsController _handsController = new HandsController();
  
You can specify whether the controller will detect the left hand (DetectLeftHand property), the right hand (DetectRightHand property), or both hands. By default, the controller tracks both hands.

Then, you'll need to subscribe to the HandsDetected event. This event is raised when a new set of hands is detected.

    _handsController.HandsDetected += HandsController_HandsDetected;
  
Then, you have to udpate the HandsController with Depth and Body data. You'll need a DepthReader and a BodyReader (check the sample project for more details).

    private void DepthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
    {
      using (DepthFrame frame = e.FrameReference.AcquireFrame())
      {
        if (frame != null)
        {
          using (KinectBuffer buffer = frame.LockImageBuffer())
          {
            _handsController.Update(buffer.UnderlyingBuffer, _body);
          }
        }
      }
    }

Finally, you can access the finger data by handling the HandsDetected event:

    private void HandsController_HandsDetected(object sender, HandCollection e)
    {
      if (e.HandLeft != null)
      {
        // Contour in the 2D depth space.
        var depthPoints = e.HandLeft.ContourDepth;
        
        // Contour in the 2D color space.
        var colorPoints = e.HandLeft.ContourColor;
        
        // Contour in the 3D camera space.
        var cameraPoints = e.HandLeft.ContourCamera;
  
        foreach (var finger in e.HandLeft.Fingers)
        {
          // Finger tip in the 2D depth space.
          var depthPoint = finger.DepthPoint;
          
          // Finger tip in the 2D color space.
          var colorPoint = finger.ColorPoint;
          
          // Finger tip in the 3D camera space.
          var cameraPoint = finger.CameraPoint;
        }
      }
  
      if (e.HandRight != null)
      {
        // Do something with the data...
      }
    }

## Contributors
* [Vangos Pterneas](http://pterneas.com) from [LightBuzz](http://lightbuzz.com)

## License
You are free to use these libraries in personal and commercial projects by attributing the original creator of the project. [View full License](https://github.com/LightBuzz/Kinect-Finger-Tracking/blob/master/LICENSE).
