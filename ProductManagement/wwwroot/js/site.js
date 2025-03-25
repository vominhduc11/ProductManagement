// Hàm hiển thị thông báo
function showToast(message, type = 'success') {
    // Tạo một phần tử div hiển thị thông báo
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    // Tạo nội dung thông báo
    const flexBox = document.createElement('div');
    flexBox.className = 'd-flex';

    const toastBody = document.createElement('div');
    toastBody.className = 'toast-body';
    toastBody.innerHTML = message;

    const closeButton = document.createElement('button');
    closeButton.type = 'button';
    closeButton.className = 'btn-close btn-close-white me-2 m-auto';
    closeButton.setAttribute('data-bs-dismiss', 'toast');
    closeButton.setAttribute('aria-label', 'Close');

    flexBox.appendChild(toastBody);
    flexBox.appendChild(closeButton);
    toast.appendChild(flexBox);

    // Tạo container cho thông báo nếu chưa có
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    // Thêm thông báo vào container
    toastContainer.appendChild(toast);

    // Hiển thị thông báo
    const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
    bsToast.show();

    // Xóa thông báo sau khi đã ẩn
    toast.addEventListener('hidden.bs.toast', function () {
        toastContainer.removeChild(toast);
    });
}

// Xác nhận trước khi xóa
function confirmDelete(event, message = 'Bạn có chắc chắn muốn xóa?') {
    if (!confirm(message)) {
        event.preventDefault();
        return false;
    }
    return true;
}

// Xem trước hình ảnh
function previewImage(input, previewId) {
    const preview = document.getElementById(previewId);
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = 'block';
        }
        reader.readAsDataURL(input.files[0]);
    } else {
        preview.src = '#';
        preview.style.display = 'none';
    }
}

// Định dạng tiền tệ
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Hàm thêm vào giỏ hàng
function addToCart(productId, quantity) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { productId: productId, quantity: quantity },
        success: function (response) {
            if (response.success) {
                // Cập nhật số lượng trong giỏ hàng
                $('.cart-count').text(response.cartCount);

                // Hiển thị thông báo
                showToast('Đã thêm sản phẩm vào giỏ hàng', 'success');
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                // Người dùng chưa đăng nhập
                showToast('Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng', 'warning');
                setTimeout(function () {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                }, 2000);
            } else {
                showToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'danger');
            }
        }
    });
}

// Hàm cập nhật số lượng sản phẩm trong giỏ hàng
function updateCartQuantity(productId, quantity) {
    $.ajax({
        url: '/Cart/UpdateQuantity',
        type: 'POST',
        data: { productId: productId, quantity: quantity },
        success: function (response) {
            if (response.success) {
                // Cập nhật tổng tiền sản phẩm
                $('.cart-item[data-product-id="' + productId + '"] .item-total').text(formatCurrency(response.itemTotal));

                // Cập nhật tổng giỏ hàng
                $('#cart-subtotal').text(formatCurrency(response.cartTotal));
                $('#cart-total').text(formatCurrency(response.cartTotal));

                // Cập nhật số lượng
                $('.cart-count').text(response.cartCount);

                // Kiểm tra nếu giỏ hàng trống
                if (response.cartCount === 0) {
                    location.reload();
                }
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                // Người dùng đã đăng xuất trong quá trình sử dụng
                showToast('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại', 'warning');
                setTimeout(function () {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                }, 2000);
            } else {
                showToast('Có lỗi xảy ra khi cập nhật giỏ hàng', 'danger');
            }
        }
    });
}

// Hiệu ứng hiển thị sản phẩm
document.addEventListener('DOMContentLoaded', function () {
    // Hiệu ứng xuất hiện cho danh sách sản phẩm
    const productCards = document.querySelectorAll('.product-grid .card');

    if (productCards.length > 0) {
        productCards.forEach((card, index) => {
            setTimeout(() => {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 100 * index);
        });
    }

    // Thêm vào giỏ hàng từ trang danh sách sản phẩm
    $(document).on('click', '.add-to-cart', function () {
        var productId = $(this).data('product-id');
        addToCart(productId, 1);
    });

    // Xử lý số lượng sản phẩm trong giỏ hàng
    $(document).on('click', '.increment-qty', function () {
        var $input = $(this).closest('.input-group').find('.item-quantity');
        var currentVal = parseInt($input.val());
        if (!isNaN(currentVal)) {
            $input.val(currentVal + 1);
            updateCartQuantity($(this).closest('.cart-item').data('product-id'), currentVal + 1);
        }
    });

    $(document).on('click', '.decrement-qty', function () {
        var $input = $(this).closest('.input-group').find('.item-quantity');
        var currentVal = parseInt($input.val());
        if (!isNaN(currentVal) && currentVal > 1) {
            $input.val(currentVal - 1);
            updateCartQuantity($(this).closest('.cart-item').data('product-id'), currentVal - 1);
        }
    });

    $(document).on('change', '.item-quantity', function () {
        var quantity = parseInt($(this).val());
        if (isNaN(quantity) || quantity < 1) {
            quantity = 1;
            $(this).val(1);
        }
        updateCartQuantity($(this).closest('.cart-item').data('product-id'), quantity);
    });

    // Xóa sản phẩm khỏi giỏ hàng
    $(document).on('click', '.remove-item', function () {
        var $cartItem = $(this).closest('.cart-item');
        var productId = $cartItem.data('product-id');

        if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
            $.ajax({
                url: '/Cart/RemoveFromCart',
                type: 'POST',
                data: { productId: productId },
                success: function (response) {
                    if (response.success) {
                        $cartItem.fadeOut(300, function () {
                            $(this).remove();

                            // Cập nhật tổng tiền
                            $('#cart-subtotal').text(formatCurrency(response.cartTotal));
                            $('#cart-total').text(formatCurrency(response.cartTotal));

                            // Cập nhật số lượng
                            $('.cart-count').text(response.cartCount);

                            // Kiểm tra nếu giỏ hàng trống
                            if (response.cartCount === 0) {
                                location.reload();
                            }
                        });
                    }
                },
                error: function (xhr) {
                    if (xhr.status === 401) {
                        // Người dùng đã đăng xuất trong quá trình sử dụng
                        showToast('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại', 'warning');
                        setTimeout(function () {
                            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                        }, 2000);
                    } else {
                        showToast('Có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng', 'danger');
                    }
                }
            });
        }
    });

    // Xóa toàn bộ giỏ hàng
    $('#clear-cart').on('click', function () {
        if (confirm('Bạn có chắc chắn muốn xóa tất cả sản phẩm khỏi giỏ hàng?')) {
            $.ajax({
                url: '/Cart/ClearCart',
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        location.reload();
                    }
                },
                error: function (xhr) {
                    if (xhr.status === 401) {
                        showToast('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại', 'warning');
                        setTimeout(function () {
                            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                        }, 2000);
                    } else {
                        showToast('Có lỗi xảy ra khi xóa giỏ hàng', 'danger');
                    }
                }
            });
        }
    });

    // Kiểm tra và hiển thị thông báo từ Query String (flash message)
    const urlParams = new URLSearchParams(window.location.search);
    const message = urlParams.get('message');
    const messageType = urlParams.get('type') || 'success';

    if (message) {
        showToast(decodeURIComponent(message), messageType);

        // Xóa thông báo khỏi URL bằng History API
        const url = new URL(window.location);
        url.searchParams.delete('message');
        url.searchParams.delete('type');
        window.history.replaceState({}, '', url);
    }
});

// CSS bổ sung cho hiệu ứng
document.addEventListener('DOMContentLoaded', function () {
    const style = document.createElement('style');
    style.textContent = `
        .product-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
            gap: 20px;
        }
        
        .product-grid .card {
            opacity: 0;
            transform: translateY(20px);
            transition: opacity 0.5s ease, transform 0.5s ease, box-shadow 0.3s;
        }
        
        .product-grid .card:hover {
            transform: translateY(-5px);
            box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15);
        }
        
        .dropdown-menu {
            display: block;
            visibility: hidden;
            opacity: 0;
            transform: translateY(10px);
            transition: visibility 0.2s, opacity 0.2s, transform 0.2s;
        }
        
        .dropdown-menu.show {
            visibility: visible;
            opacity: 1;
            transform: translateY(0);
        }
        
        .toast-container {
            z-index: 9999;
        }
        
        .toast {
            transition: opacity 0.3s ease-in-out;
        }
        
        .category-badge {
            background-color: #1abc9c;
            color: white;
            padding: 0.25rem 0.5rem;
            border-radius: 20px;
            font-size: 0.8rem;
            font-weight: 500;
        }
        
        .price {
            font-weight: 700;
            color: #3498db;
            font-size: 1.25rem;
        }
    `;

    document.head.appendChild(style);
});