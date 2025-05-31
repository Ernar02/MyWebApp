$(function () {
    $('#selectAll').on('change', function () {
        var checked = $(this).is(':checked');
        $('.user-checkbox').prop('checked', checked);
    });

    $('.user-checkbox').on('change', function () {
        if (!$(this).is(':checked')) {
            $('#selectAll').prop('checked', false);
        } else {
            const allChecked = $('.user-checkbox').length === $('.user-checkbox:checked').length;
            $('#selectAll').prop('checked', allChecked);
        }
    });

    $('#filterInput').on('input', function () {
        var filter = $(this).val().toLowerCase().trim();
        $('.user-row').each(function () {
            var name = $(this).data('name');
            var email = $(this).data('email');
            if (name.includes(filter) || email.includes(filter)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    $('#blockBtn').on('click', function () {
        const ids = getSelectedUserIds();
        if (ids.length > 0) {
            postAction('/Admin/BlockUsers', ids);
        } 
    });

    $('#thumbsBtn').on('click', function () {
        const ids = getSelectedUserIds();
        if (ids.length > 0) {
            postAction('/Admin/UnblockUsers', ids);
        } 
    });

    $('#deleteBtn').on('click', function () {
        const ids = getSelectedUserIds();
        if (ids.length > 0) {
            {
                postAction('/Admin/DeleteUsers', ids);
            }
        } 
    });

    function getSelectedUserIds() {
        return $('.user-checkbox:checked').map(function () {
            return $(this).data('user-id');
        }).get();
    }

    async function postAction(url, userIds) {
        try {
            const token = $('input[name="__RequestVerificationToken"]').val();
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(userIds)
            });

            if (response.ok) {
                location.reload();
            }
            
        } catch (error) {
            console.error('Error performing action:', error);
            alert("An error occurred while performing the action.");
        }
    }
});