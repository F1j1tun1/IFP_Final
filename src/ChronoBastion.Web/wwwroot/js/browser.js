export function watchVisibility(reference) {
    const changed = () => {
        if (document.hidden)
            reference.invokeMethodAsync("OnHidden").catch(() => {});
    };
    document.addEventListener("visibilitychange", changed);
    changed();
    return {
        dispose: () =>
            document.removeEventListener("visibilitychange", changed),
    };
}

// Keep keyboard focus inside the currently open dialog and restore its trigger.
export function watchDialogs() {
    let current = null;
    let previous = null;
    const observer = new MutationObserver(() => {
        const next = document.querySelector("[data-dialog]");
        if (next === current) return;
        if (next) {
            if (!current) previous = document.activeElement;
            current = next;
            next.focus();
        } else {
            current = null;
            if (previous?.isConnected) previous.focus();
            previous = null;
        }
    });
    const keydown = (event) => {
        if (!current || event.key !== "Tab") return;
        const items = [
            ...current.querySelectorAll(
                'button:not(:disabled), select, a[href], [tabindex="0"]',
            ),
        ];
        if (!items.length) {
            event.preventDefault();
            return;
        }
        const first = items[0],
            last = items.at(-1);
        if (
            event.shiftKey &&
            (document.activeElement === first ||
                document.activeElement === current)
        ) {
            event.preventDefault();
            last.focus();
        } else if (
            !event.shiftKey &&
            (document.activeElement === last ||
                document.activeElement === current)
        ) {
            event.preventDefault();
            first.focus();
        }
    };
    observer.observe(document.body, { childList: true, subtree: true });
    document.addEventListener("keydown", keydown);
    return {
        dispose: () => {
            observer.disconnect();
            document.removeEventListener("keydown", keydown);
        },
    };
}
