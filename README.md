# RealSense Emotional sensing project

HERE BE FUGLY DRAGONS 

###You will need: 

1. A RealSense camera, Windows 8.1+, USB 3.0, RealSense SDK, Visual C# (or similar)
2. Open and run the project, currently it just posts to an API


### Info

Main logic is inside EmotionalDetection.cs file. 

I'm using DisplayLocation as an Update method because I'm a bad person. 

We post EmotionalResponses inside EmotionalResponseArrays as jsons, and make Jsons just by having a toJson instead of using a proper library. 