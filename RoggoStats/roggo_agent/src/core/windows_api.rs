#[cfg(windows)]
pub fn make_socket_not_inheritable<T>(socket: &T) -> std::io::Result<()>
where
    T: std::os::windows::io::AsRawSocket,
{
    use windows_sys::Win32::Foundation::{
        SetHandleInformation, HANDLE, HANDLE_FLAG_INHERIT,
    };

    let handle = socket.as_raw_socket() as HANDLE;

    let ok = unsafe {
        SetHandleInformation(
            handle,
            HANDLE_FLAG_INHERIT,
            0,
        )
    };

    if ok == 0 {
        return Err(std::io::Error::last_os_error());
    }

    Ok(())
}

#[cfg(not(windows))]
pub fn make_socket_not_inheritable<T>(_socket: &T) -> std::io::Result<()> {
    Ok(())
}