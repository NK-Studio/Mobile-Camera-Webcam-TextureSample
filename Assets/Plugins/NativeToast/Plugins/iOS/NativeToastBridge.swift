@_cdecl("MakeToast")
func MakeToast(messagePtr: UnsafePointer<CChar>) {
    let message = String(cString: messagePtr)
    NativeToast.shared.MakeToast(message: message)
}
