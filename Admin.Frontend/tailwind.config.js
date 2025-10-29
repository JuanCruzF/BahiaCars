/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./**/*.{razor,html,js,jsx,ts,tsx}"], // <-- AÑADE ESTO
    theme: {
        extend: {
            keyframes: {
                'fade-in-up': {
                    '0%': {
                        opacity: '0',
                        transform: 'translateY(10px)'
                    },
                    '100%': {
                        opacity: '1',
                        transform: 'translateY(0)'
                    },
                },
                'shake': {
                    '0%, 100%': { transform: 'translateX(0)' },
                    '10%, 30%, 50%, 70%, 90%': { transform: 'translateX(-5px)' },
                    '20%, 40%, 60%, 80%': { transform: 'translateX(5px)' }
                }
            },
            animation: {
                'fade-in-up': 'fade-in-up 0.5s ease-out',
                'shake': 'shake 0.5s ease-in-out'
            }
        },
    },
    plugins: [],
}