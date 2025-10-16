use clap_complete::{generate, Generator, Shell as ClapShell};
use std::io;

use crate::cli::{Cli, Shell};

pub fn generate(shell: Shell) {
    let mut cmd = Cli::command();
    let name = cmd.get_name().to_string();
    
    let clap_shell = match shell {
        Shell::Bash => ClapShell::Bash,
        Shell::Fish => ClapShell::Fish,
        Shell::Zsh => ClapShell::Zsh,
        Shell::PowerShell => ClapShell::PowerShell,
        Shell::Elvish => ClapShell::Elvish,
    };
    
    generate_completion(clap_shell, &mut cmd, name, &mut io::stdout());
}

fn generate_completion<G: Generator>(gen: G, cmd: &mut clap::Command, name: String, buf: &mut dyn io::Write) {
    generate(gen, cmd, name, buf);
}
