(() => {
    const getShell = () => ({
        menu: document.querySelector(".menu-container"),
        actions: document.querySelector(".header-actions"),
        menuToggle: document.querySelector('[data-shell-toggle="menu"]'),
        actionsToggle: document.querySelector('[data-shell-toggle="actions"]')
    });

    const setExpanded = (button, expanded) => button?.setAttribute("aria-expanded", String(expanded));

    const closeActions = shell => {
        shell.actions?.classList.remove("is-open");
        shell.actions?.querySelectorAll("details[open]").forEach(details => details.removeAttribute("open"));
        setExpanded(shell.actionsToggle, false);
    };

    const closeShell = () => {
        const shell = getShell();
        shell.menu?.classList.remove("is-open");
        closeActions(shell);
        setExpanded(shell.menuToggle, false);
    };

    document.addEventListener("click", event => {
        if (!(event.target instanceof Element)) return;

        const shell = getShell();
        const menuToggle = event.target.closest('[data-shell-toggle="menu"]');
        const actionsToggle = event.target.closest('[data-shell-toggle="actions"]');

        if (menuToggle) {
            const shouldOpen = !shell.menu?.classList.contains("is-open");
            closeActions(shell);
            shell.menu?.classList.toggle("is-open", shouldOpen);
            setExpanded(shell.menuToggle, shouldOpen);
            return;
        }

        if (actionsToggle) {
            const shouldOpen = !shell.actions?.classList.contains("is-open");
            shell.menu?.classList.remove("is-open");
            if (shouldOpen) shell.actions?.classList.add("is-open");
            else closeActions(shell);
            setExpanded(shell.actionsToggle, shouldOpen);
            setExpanded(shell.menuToggle, false);
            return;
        }

        if (event.target.closest("[data-shell-close]")) {
            closeShell();
            return;
        }

        if (shell.actions?.classList.contains("is-open") && !event.target.closest("#header-actions")) {
            closeActions(shell);
        }
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape") closeShell();
    });

    document.addEventListener("blazor:enhancedload", closeShell);
})();
