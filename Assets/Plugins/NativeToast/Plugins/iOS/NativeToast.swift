import Toast

public class NativeToast: NSObject {
    
    public static let shared = NativeToast()
    
    public func MakeToast(message: String) {
        GetAppController().rootView.makeToast(message)
    }
}
