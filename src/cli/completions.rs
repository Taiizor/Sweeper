use clap::CommandFactory;
use clap_complete::{generate as clap_generate, Shell as ClapShell};
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
    
    clap_generate(clap_shell, &mut cmd, name, &mut io::stdout());
}