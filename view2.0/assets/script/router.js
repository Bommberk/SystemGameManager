async function loadPage(page) 
{
    const response = await fetch(`pages/${page}.html`);
    const html = await response.text();

    document.getElementById("page").innerHTML = html;
    let sidebarButton = "sidebar"+page.charAt(0).toUpperCase()+page.slice(1)+"Button";
    let previous = document.querySelector("#sidebar li.active");
    if (previous) {
        previous.classList.remove("active");
    }
    document.querySelector("#"+sidebarButton)?.classList.add("active");
}

async function loadSidebar()
{
    const response = await fetch("components/sidebar.html");
    const html = await response.text();

    document.getElementById("sidebar").innerHTML = html;
}