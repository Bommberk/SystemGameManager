async function loadPage(page) 
{
    const response = await fetch(`pages/${page}.html`);
    const html = await response.text();

    document.getElementById("page").innerHTML = html;
}

async function loadSidebar()
{
    const response = await fetch("components/sidebar.html");
    const html = await response.text();

    document.getElementById("sidebar").innerHTML = html;
}