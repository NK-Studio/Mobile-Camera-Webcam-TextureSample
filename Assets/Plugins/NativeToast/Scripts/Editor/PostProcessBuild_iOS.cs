#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostProcessBuild_iOS
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            string unityIphoneTargetGuid = pbxProject.GetUnityMainTargetGuid();
            string unityFrameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
            
            string packageFrameworkName = "Toast";
            string packageUrl = "https://github.com/scalessec/Toast-Swift.git";
            string version = "5.1.1";

            var toastGuid = pbxProject.AddRemotePackageReferenceAtVersion(packageUrl, version);
            pbxProject.AddRemotePackageFrameworkToProject(unityIphoneTargetGuid, packageFrameworkName, toastGuid, false);
            pbxProject.AddRemotePackageFrameworkToProject(unityFrameworkTargetGuid, packageFrameworkName, toastGuid, false);

            File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());
        }
    }
}
#endif