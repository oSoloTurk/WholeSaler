function pageSizeChange(select) {
    var action = document.getElementById("applyPageSizeChange");
    if (action.getAttribute("href").includes("pageSize")) {
        var value = parseInt(action.innerText);
        action.setAttribute("href", action.getAttribute("href").replace("pageSize=" + value, "pageSize=" + select.value));
    } else {
        action.setAttribute("href", action.getAttribute("href") + "/?pageSize=" + select.value);
    }
    action.click();
}