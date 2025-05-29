// --- Global Data (for simplicity; in a real app, this would come from an API) ---
const ALL_BOOKS = [
    { id: '1', title: 'Suç ve Ceza', author: 'Fyodor Dostoyevski', price: 35.99, category: 'roman' },
    { id: '2', title: '1984', author: 'George Orwell', price: 29.99, category: 'roman' },
    { id: '3', title: 'Simyacı', author: 'Paulo Coelho', price: 24.99, category: 'roman' },
    { id: '4', title: 'Sapiens', author: 'Yuval Noah Harari', price: 42.99, category: 'bilim' },
    { id: '5', title: 'Kürk Mantolu Madonna', author: 'Sabahattin Ali', price: 19.99, category: 'roman' },
    { id: '6', title: 'İnsan Nasıl Yaşar', author: 'Lev Tolstoy', price: 27.99, category: 'kisisel-gelisim' },
    { id: '7', title: 'Evrenin Kısa Tarihi', author: 'Stephen Hawking', price: 39.50, category: 'bilim' },
    { id: '8', title: 'Sanatın Öyküsü', author: 'E. H. Gombrich', price: 55.00, category: 'sanat' },
    { id: '9', title: 'Etkili İnsanların 7 Alışkanlığı', author: 'Stephen Covey', price: 32.00, category: 'is-kariyer' },
    { id: '10', title: 'Nutuk', author: 'Mustafa Kemal Atatürk', price: 45.00, category: 'tarih' },
    { id: '11', title: 'Küçük Prens', author: 'Antoine de Saint-Exupéry', price: 18.50, category: 'cocuk' },
    { id: '12', title: 'Hobbit', author: 'J.R.R. Tolkien', price: 30.00, category: 'fantastik' },
    { id: '13', title: 'Psikolojinin Temelleri', author: 'Carole Wade', price: 60.00, category: 'bilim' },
    { id: '14', title: 'Sıfırıncı Gün', author: 'Sabahattin Zaim', price: 22.00, category: 'is-kariyer' },
    { id: '15', title: 'Dönüşüm', author: 'Franz Kafka', price: 15.00, category: 'roman' }
];

const QUOTES = [
    { quote: "Okumak, bilginin en derin kuyularından kana kana içmek gibidir. Her yeni sayfa, yeni bir dünyaya açılan kapıdır.", author: "Mevlana" },
    { quote: "İnsan ruhunun gıdası kitaplardır. Okudukça büyür, gelişimini sürdürür ve hayatı daha derinden anlar.", author: "Fyodor Dostoyevski" },
    { quote: "Bir kitap, uykusuz bir gecenin ve sonsuz bir düşüncenin ürünüdür.", author: "George Orwell" },
    { quote: "En iyi arkadaşım, bana okumadığım bir kitabı veren kişidir.", author: "Abraham Lincoln" },
    { quote: "Kitaplar, hiç kapanmayan pencerelerdir.", author: "Nazım Hikmet" },
    { quote: "Okumadan geçen bir gün, kaybolmuş bir gündür.", author: "Anonim" }
];

// --- Cart Functions (Global) ---
let cart = JSON.parse(localStorage.getItem('kitapDunyasiCart')) || [];

function updateCartCountDisplay() {
    const cartItemCountSpans = document.querySelectorAll('.cart-count'); // Select all cart count spans
    const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
    cartItemCountSpans.forEach(span => {
        span.textContent = totalItems;
        // Add a bounce animation class if items are added
        if (totalItems > 0 && span.classList.contains('animated-once')) { // To prevent constant bouncing
            span.classList.remove('animated-once');
        } else if (totalItems > 0 && !span.classList.contains('bounce')) {
            span.classList.add('bounce');
            setTimeout(() => span.classList.remove('bounce'), 500);
        }
    });
}

function saveCart() {
    localStorage.setItem('kitapDunyasiCart', JSON.stringify(cart));
    updateCartCountDisplay();
}

function addToCart(bookId) {
    const bookToAdd = ALL_BOOKS.find(book => book.id === bookId);
    if (bookToAdd) {
        const existingBook = cart.find(item => item.id === bookId);
        if (existingBook) {
            existingBook.quantity++;
        } else {
            cart.push({ ...bookToAdd, quantity: 1 });
        }
        saveCart();
        alert(`"${bookToAdd.title}" sepete eklendi!`);
    }
}

function readBook(bookTitle) {
    alert(`"${bookTitle}" kitabını okumaya başlıyorsunuz... Keyifli okumalar!`);
}


// --- DOMContentLoaded for all pages ---
document.addEventListener('DOMContentLoaded', () => {
    updateCartCountDisplay(); // Initialize cart count on all pages

    // Smooth scrolling for navigation links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Handle cart icon click to navigate to cart page
    document.querySelectorAll('.cart-container').forEach(cartIcon => {
        cartIcon.addEventListener('click', (e) => {
            // Only navigate if it's not already the cart page
            if (window.location.pathname.indexOf('/cart.html') === -1 && window.location.pathname.indexOf('/Home/Sepetim') === -1) {
                window.location.href = 'cart.html'; // Or '/Home/Sepetim' if using ASP.NET MVC routing
            }
        });
    });

    // --- Index Page Specific Logic ---
    if (document.body.id === 'index-page') {
        // Hero section animations (already CSS-based, ensure they run)
        const heroTitle = document.querySelector('.hero h1');
        const heroSubtitle = document.querySelector('.hero p');
        const ctaButtons = document.querySelector('.cta-buttons');
        if (heroTitle && heroSubtitle && ctaButtons) {
            heroTitle.style.animationPlayState = 'running';
            heroSubtitle.style.animationPlayState = 'running';
            ctaButtons.style.animationPlayState = 'running';
        }

        // Quote of the Day functionality
        let currentQuoteIndex = 0;
        const dailyQuoteElement = document.getElementById('daily-quote');
        const quoteAuthorElement = document.getElementById('quote-author');
        const newQuoteButton = document.getElementById('new-quote-btn');

        function displayQuote() {
            if (!dailyQuoteElement || !quoteAuthorElement) return;

            dailyQuoteElement.classList.add('fade-out');
            quoteAuthorElement.classList.add('fade-out');

            setTimeout(() => {
                const quote = QUOTES[currentQuoteIndex];
                dailyQuoteElement.textContent = `"${quote.quote}"`;
                quoteAuthorElement.textContent = `- ${quote.author}`;

                dailyQuoteElement.classList.remove('fade-out');
                quoteAuthorElement.classList.remove('fade-out');
                dailyQuoteElement.classList.add('fade-in-quote');
                quoteAuthorElement.classList.add('fade-in-quote');

                setTimeout(() => {
                    dailyQuoteElement.classList.remove('fade-in-quote');
                    quoteAuthorElement.classList.remove('fade-in-quote');
                }, 1000);
            }, 300); // Wait for fade-out to complete

            currentQuoteIndex = (currentQuoteIndex + 1) % QUOTES.length;
        }

        // Add dynamic styles for quote animation
        const styleSheet = document.createElement("style");
        styleSheet.type = "text/css";
        styleSheet.innerText = `
            .fade-out { opacity: 0; transition: opacity 0.3s ease-out; }
            .fade-in-quote { animation: fadeIn 1s forwards; }
        `;
        document.head.appendChild(styleSheet);

        if (newQuoteButton) {
            newQuoteButton.addEventListener('click', displayQuote);
        }
        displayQuote(); // Display initial quote on load

        // Intersection Observer for section animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                    entry.target.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        document.querySelectorAll('.category-card, .testimonial-card, .quote-of-the-day').forEach(el => {
            el.style.opacity = '0';
            el.style.transform = 'translateY(20px)';
            observer.observe(el);
        });

        // Initialize book cards for index.html if present
        document.querySelectorAll('.featured-books .book-card').forEach(card => {
            const bookId = card.dataset.bookId;
            const bookName = card.querySelector('.book-image').textContent;
            const bookPrice = parseFloat(card.querySelector('.book-price').textContent.replace('₺', ''));

            const buyButton = card.querySelector('.btn-buy');
            if (buyButton) {
                buyButton.addEventListener('click', () => addToCart(bookId));
            }
            const readButton = card.querySelector('.btn-read');
            if (readButton) {
                readButton.addEventListener('click', () => readBook(bookName));
            }
        });


    }

    // --- Books Page Specific Logic ---
    if (document.body.id === 'books-page') {
        const booksGrid = document.getElementById('books-grid');
        const noResults = document.getElementById('no-results');
        const searchInput = document.getElementById('search-input');
        const searchBtn = document.getElementById('search-btn');
        const categoryBtns = document.querySelectorAll('.category-btn');

        let currentCategory = 'all';
        let currentSearch = '';

        function performSearch() {
            currentSearch = searchInput.value.trim().toLowerCase();
            filterAndRenderBooks();
        }

        function filterAndRenderBooks() {
            let filtered = ALL_BOOKS;

            // Category filter
            if (currentCategory !== 'all') {
                filtered = filtered.filter(book => book.category === currentCategory);
            }

            // Search filter
            if (currentSearch) {
                filtered = filtered.filter(book =>
                    book.title.toLowerCase().includes(currentSearch) ||
                    book.author.toLowerCase().includes(currentSearch)
                );
            }

            renderBooks(filtered);
        }

        function renderBooks(booksToRender) {
            if (booksToRender.length === 0) {
                booksGrid.style.display = 'none';
                noResults.style.display = 'flex'; // Use flex for centering icon/text
                return;
            }

            booksGrid.style.display = 'grid';
            noResults.style.display = 'none';

            booksGrid.innerHTML = booksToRender.map((book, index) => `
                <div class="book-card" data-book-id="${book.id}" style="animation-delay: ${index * 0.05}s;">
                    <div class="book-image">
                        ${book.title}
                    </div>
                    <div class="book-content">
                        <h3 class="book-title">${book.title}</h3>
                        <p class="book-author">${book.author}</p>
                        <div class="book-price">₺${book.price.toFixed(2)}</div>
                        <div class="book-actions">
                            <button class="btn btn-primary add-to-cart-btn" data-book-id="${book.id}">
                                <i class="fas fa-shopping-cart"></i> Sepete Ekle
                            </button>
                            <button class="btn btn-secondary read-book-btn" data-book-title="${book.title}">
                                <i class="fas fa-book-open"></i> Oku
                            </button>
                        </div>
                    </div>
                </div>
            `).join('');

            // Attach event listeners to newly rendered buttons
            document.querySelectorAll('.add-to-cart-btn').forEach(button => {
                button.addEventListener('click', (e) => {
                    const bookId = e.currentTarget.dataset.bookId;
                    addToCart(bookId);
                });
            });

            document.querySelectorAll('.read-book-btn').forEach(button => {
                button.addEventListener('click', (e) => {
                    const bookTitle = e.currentTarget.dataset.bookTitle;
                    readBook(bookTitle);
                });
            });
        }

        // Attach event listeners for books page
        categoryBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                categoryBtns.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                currentCategory = btn.dataset.category;
                filterAndRenderBooks();
            });
        });

        searchBtn.addEventListener('click', performSearch);
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') performSearch();
        });

        filterAndRenderBooks(); // Initial render for books page
    }

    // --- Cart Page Specific Logic ---
    if (document.body.id === 'cart-page') {
        const cartItemsContainer = document.getElementById('cart-items-container');
        const cartSummary = document.getElementById('cart-summary');
        const booksSubtotalPriceSpan = document.getElementById('books-subtotal-price');
        const vatAmountSpan = document.getElementById('vat-amount');
        const shippingFeeSpan = document.getElementById('shipping-fee');
        const finalTotalPriceSpan = document.getElementById('final-total-price');
        const checkoutButton = document.getElementById('checkout-button');
        const emptyCartMessage = document.getElementById('empty-cart-message');

        const VAT_RATE = 0.18; // %18 KDV
        const SHIPPING_FEE = 15.00; // Sabit kargo ücreti

        function renderCartItems() {
            cartItemsContainer.innerHTML = '';
            let booksSubtotal = 0;

            if (cart.length === 0) {
                emptyCartMessage.style.display = 'block';
                cartSummary.style.display = 'none';
                checkoutButton.style.display = 'none';
                return;
            } else {
                emptyCartMessage.style.display = 'none';
                cartSummary.style.display = 'block';
                checkoutButton.style.display = 'block';
            }

            cart.forEach(item => {
                const itemTotal = item.price * item.quantity;
                booksSubtotal += itemTotal;

                const cartItemDiv = document.createElement('div');
                cartItemDiv.classList.add('cart-item');
                cartItemDiv.dataset.bookId = item.id;

                cartItemDiv.innerHTML = `
                    <div class="cart-item-image">${item.title.substring(0, 10) + (item.title.length > 10 ? '...' : '')}</div>
                    <div class="cart-item-details">
                        <div class="cart-item-title">${item.title}</div>
                        <div class="cart-item-price">₺${item.price.toFixed(2)}</div>
                    </div>
                    <div class="cart-item-quantity">
                        <button data-action="decrease">-</button>
                        <span>${item.quantity}</span>
                        <button data-action="increase">+</button>
                    </div>
                    <button class="remove-item-btn" data-action="remove"><i class="fas fa-trash-alt"></i> Sil</button>
                `;
                cartItemsContainer.appendChild(cartItemDiv);
            });

            // Calculate and display totals
            const vatAmount = booksSubtotal * VAT_RATE;
            const finalTotal = booksSubtotal + vatAmount + SHIPPING_FEE;

            booksSubtotalPriceSpan.textContent = `₺${booksSubtotal.toFixed(2)}`;
            vatAmountSpan.textContent = `₺${vatAmount.toFixed(2)}`;
            shippingFeeSpan.textContent = `₺${SHIPPING_FEE.toFixed(2)}`;
            finalTotalPriceSpan.textContent = `₺${finalTotal.toFixed(2)}`;
        }

        function handleCartActions(event) {
            const target = event.target;
            const bookId = target.closest('.cart-item')?.dataset.bookId;
            if (!bookId) return;

            const existingBookIndex = cart.findIndex(item => item.id === bookId);
            if (existingBookIndex === -1) return;

            if (target.dataset.action === 'increase') {
                cart[existingBookIndex].quantity++;
            } else if (target.dataset.action === 'decrease') {
                if (cart[existingBookIndex].quantity > 1) {
                    cart[existingBookIndex].quantity--;
                } else {
                    cart.splice(existingBookIndex, 1);
                }
            } else if (target.dataset.action === 'remove') {
                cart.splice(existingBookIndex, 1);
            }

            saveCart(); // This calls updateCartCountDisplay and renderCartItems
        }

        if (cartItemsContainer) {
            cartItemsContainer.addEventListener('click', handleCartActions);
        }

        if (checkoutButton) {
            checkoutButton.addEventListener('click', (e) => {
                e.preventDefault();
                if (cart.length > 0) {
                    alert("Siparişiniz başarıyla alındı! Teşekkür ederiz.");
                    cart = [];
                    saveCart(); // Clear cart and update display
                } else {
                    alert("Sepetiniz boş. Lütfen önce ürün ekleyin.");
                }
            });
        }
        renderCartItems(); // Initial render for cart page
    }
});