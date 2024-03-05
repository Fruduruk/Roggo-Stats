fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("Compiling proto files...");
    tonic_build::compile_protos("proto/ballchasing.proto")?;
    Ok(())
}