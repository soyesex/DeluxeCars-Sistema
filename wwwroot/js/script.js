// Carrito
let cart = [];
let products = [];

// Función para mostrar los productos en el catálogo
function displayProducts(productsData) {
    products = productsData;
    const catalog = document.getElementById("product-catalog");
    catalog.innerHTML = "";
    products.forEach(product => {
        const card = document.createElement("div");
        card.className = "col-md-4 mb-4";
        card.innerHTML = `
            <div class="card">
                <img src="${product.imagenUrl}" alt="${product.nombre}" class="card-img-top" asp-append-version="true">
                <div class="card-body">
                    <h5 class="card-title">${product.nombre}</h5>
                    <p class="card-text">$${product.precio.toFixed(2)}</p>
                    <p class="card-text"><small class="text-muted">${product.descripcion}</small></p>
                    <p class="card-text"><small class="text-muted">Stock: ${product.stock}</small></p>
                    <button class="btn btn-success" onclick="addToCart(${product.id})" ${product.stock === 0 ? 'disabled' : ''}>
                        ${product.stock === 0 ? 'Sin Stock' : 'Agregar al Carrito'}
                    </button>
                </div>
            </div>
        `;
        catalog.appendChild(card);
    });
}

// Función para agregar un producto al carrito
function addToCart(productId) {
    const product = products.find(p => p.id === productId);
    if (product && product.stock > 0) {
        cart.push(product);
        updateCart();
        // Abrir el carrito automáticamente al agregar un ítem
        document.getElementById("cart").classList.add("open");
    }
}

// Función para actualizar el carrito
function updateCart() {
    const cartItems = document.getElementById("cart-items");
    const cartTotal = document.getElementById("cart-total");
    const cartCount = document.getElementById("cart-count"); // Contador del carrito
    cartItems.innerHTML = ""; // Limpiar la lista
    let total = 0;

    cart.forEach((item, index) => {
        const li = document.createElement("li");
        li.className = "list-group-item d-flex justify-content-between align-items-center";
        li.innerHTML = `
            ${item.nombre} - $${item.precio.toFixed(2)}
            <button class="btn btn-sm btn-danger" onclick="removeFromCart(${index})">Eliminar</button>
        `;
        cartItems.appendChild(li);
        total += item.precio;
    });

    cartTotal.textContent = total.toFixed(2);

    cartCount.textContent = cart.length;
    // Guardar el carrito en localStorage
    localStorage.setItem('cart', JSON.stringify(cart));
}

// Función para eliminar un producto del carrito
function removeFromCart(index) {
    cart.splice(index, 1);
    updateCart();
}

// Función para abrir/cerrar el carrito
function toggleCart() {
    const cart = document.getElementById("cart");
    cart.classList.toggle("open");
}

// Función para cargar los productos usando AJAX
function loadProducts() {
    $.ajax({
        url: '/Catalogo/GetProducts',
        type: 'GET',
        success: function (data) {
            displayProducts(data);
        },
        error: function (err) {
            console.error("Error al cargar los productos", err);
        }
    });
}

// Llamada para cargar los productos cuando la página se carga
window.onload = function () {
    loadProducts();
    // Cargar el carrito desde localStorage si existe
    const savedCart = localStorage.getItem('cart');
    if (savedCart) {
        cart = JSON.parse(savedCart);
        updateCart();
    }
};

// Función para preparar los datos del carrito antes de enviar el formulario
function prepareCartData() {
    const cartItemsInput = document.getElementById("cartItemsInput");
    cartItemsInput.value = JSON.stringify(cart);
    console.log("Datos enviados en el formulario:", cartItemsInput.value);
}


