use roggo_agent::runtimes::console;
fn main() -> anyhow::Result<()> {
    console::run()?;
    Ok(())
}
