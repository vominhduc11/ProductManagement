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

// Xác thực form sử dụng JavaScript
function validateProductForm() {
    const form = document.getElementById('productForm');

    if (!form) return true;

    const name = form.querySelector('input[name="Name"]');
    const price = form.querySelector('input[name="Price"]');
    const category = form.querySelector('select[name="CategoryId"]');

    let isValid = true;

    // Xác thực tên sản phẩm
    if (!name.value.trim()) {
        showInvalidFeedback(name, 'Vui lòng nhập tên sản phẩm');
        isValid = false;
    } else {
        showValidFeedback(name);
    }

    // Xác thực giá
    if (!price.value || isNaN(price.value) || parseFloat(price.value) < 0) {
        showInvalidFeedback(price, 'Vui lòng nhập giá hợp lệ');
        isValid = false;
    } else {
        showValidFeedback(price);
    }

    // Xác thực danh mục
    if (!category.value) {
        showInvalidFeedback(category, 'Vui lòng chọn danh mục');
        isValid = false;
    } else {
        showValidFeedback(category);
    }

    return isValid;
}

function showInvalidFeedback(element, message) {
    element.classList.add('is-invalid');
    element.classList.remove('is-valid');

    // Tìm hoặc tạo phần tử hiển thị thông báo lỗi
    let feedback = element.nextElementSibling;
    if (!feedback || !feedback.classList.contains('invalid-feedback')) {
        feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        element.parentNode.insertBefore(feedback, element.nextSibling);
    }

    feedback.textContent = message;
}

function showValidFeedback(element) {
    element.classList.add('is-valid');
    element.classList.remove('is-invalid');
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

    // Hiệu ứng dropdown
    const dropdowns = document.querySelectorAll('.dropdown-toggle');
    dropdowns.forEach(dropdown => {
        dropdown.addEventListener('mouseenter', function () {
            this.classList.add('show');
            this.setAttribute('aria-expanded', 'true');
            this.nextElementSibling.classList.add('show');
        });

        const dropdownParent = dropdown.parentElement;
        dropdownParent.addEventListener('mouseleave', function () {
            dropdown.classList.remove('show');
            dropdown.setAttribute('aria-expanded', 'false');
            dropdown.nextElementSibling.classList.remove('show');
        });
    });

    // Xử lý sự kiện cho các nút xóa
    const deleteButtons = document.querySelectorAll('.delete-button');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const name = this.getAttribute('data-name');
            const confirmMessage = `Bạn có chắc chắn muốn xóa "${name}"?`;
            if (!confirm(confirmMessage)) {
                e.preventDefault();
            }
        });
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

    // Xác thực form sản phẩm
    const productForm = document.getElementById('productForm');
    if (productForm) {
        productForm.addEventListener('submit', function (e) {
            if (!validateProductForm()) {
                e.preventDefault();
                showToast('Vui lòng kiểm tra thông tin sản phẩm', 'danger');
            }
        });
    }
});

// CSS bổ sung cho hiệu ứng
document.addEventListener('DOMContentLoaded', function () {
    const style = document.createElement('style');
    style.textContent = `
        .product-grid .card {
            opacity: 0;
            transform: translateY(20px);
            transition: opacity 0.5s ease, transform 0.5s ease;
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
    `;

    document.head.appendChild(style);
});