using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // Para o delay da IA

namespace JOGO
{
    public partial class MainWindow : Window
    {
        private char[,] board = new char[3, 3];
        private bool playerXTurn = true; // True para X, False para O
        private int turnCount = 0; // Para verificar empate

        private bool isPvPMode = true; // True para 2 jogadores, False para contra o computador
        private Random random = new Random(); // Para a IA aleatória

        public MainWindow()
        {
            InitializeComponent();
            ShowModeSelection(); // Mostra a tela de seleção de modo ao iniciar
        }

        private void ShowModeSelection()
        {
            modeSelectionPanel.Visibility = Visibility.Visible;
            mainGameGrid.Visibility = Visibility.Collapsed;
        }

        private void StartGame()
        {
            modeSelectionPanel.Visibility = Visibility.Collapsed;
            mainGameGrid.Visibility = Visibility.Visible;
            InitializeGame();
        }

        private void PlayAgainstFriend_Click(object sender, RoutedEventArgs e)
        {
            isPvPMode = true;
            StartGame();
        }

        private void PlayAgainstComputer_Click(object sender, RoutedEventArgs e)
        {
            isPvPMode = false;
            StartGame();
        }

        private void InitializeGame()
        {
            // Limpa o tabuleiro lógico
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    board[r, c] = ' ';
                }
            }

            // Limpa o conteúdo de todos os botões e os habilita
            foreach (UIElement element in mainGameGrid.Children) // Alterado para mainGameGrid
            {
                if (element is Button button && button.Name.StartsWith("btn"))
                {
                    button.Content = "";
                    button.IsEnabled = true;
                }
            }

            playerXTurn = true; // X sempre começa
            turnCount = 0;
            statusTextBlock.Text = "Vez do Jogador X";

            // Se for modo contra o computador e o computador for o primeiro a jogar (por exemplo, sempre 'O'),
            // você chamaria a jogada da IA aqui. Para o nosso caso, X sempre começa e é o jogador humano.
            // Se você quiser que o computador possa ser X, precisaria de uma variável adicional.
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            // Evita jogar em um botão já preenchido ou se o jogo já terminou
            if (button.Content.ToString() != "" || CheckForWinner() || IsBoardFull())
            {
                return;
            }

            // Garante que o jogador X (humano) jogue primeiro em modo PvE
            if (!isPvPMode && !playerXTurn) // Se for modo PvE e não for a vez de X
            {
                return; // Impede o clique do usuário quando é vez da IA
            }

            // Extrai as coordenadas do nome do botão (ex: btn00 -> row=0, col=0)
            int row = int.Parse(button.Name.Substring(3, 1));
            int col = int.Parse(button.Name.Substring(4, 1));

            // Faz a jogada do jogador atual (humano)
            MakeMove(button, row, col);

            // Verifica o estado do jogo após a jogada humana
            if (CheckForWinner())
            {
                statusTextBlock.Text = $"Jogador {(playerXTurn ? "X" : "O")} venceu!";
                DisableAllButtons();
            }
            else if (IsBoardFull())
            {
                statusTextBlock.Text = "Empate!";
                DisableAllButtons();
            }
            else
            {
                playerXTurn = !playerXTurn; // Troca o turno
                statusTextBlock.Text = $"Vez do Jogador {(playerXTurn ? "X" : "O")}";

                // Se for modo contra o computador e agora for o turno do computador ('O')
                if (!isPvPMode && !playerXTurn)
                {
                    // Pequeno delay para simular "pensamento" da IA
                    await System.Threading.Tasks.Task.Delay(500);
                    ComputerMove();
                }
            }
        }

        private void MakeMove(Button button, int row, int col)
        {
            if (playerXTurn)
            {
                button.Content = "X";
                board[row, col] = 'X';
            }
            else
            {
                button.Content = "O";
                board[row, col] = 'O';
            }
            turnCount++;
        }

        private void ComputerMove()
        {
            // Encontra todos os movimentos válidos
            List<(int, int)> availableMoves = new List<(int, int)>();
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (board[r, c] == ' ')
                    {
                        availableMoves.Add((r, c));
                    }
                }
            }

            if (availableMoves.Count > 0)
            {
                // Escolhe um movimento aleatório
                int index = random.Next(availableMoves.Count);
                (int row, int col) = availableMoves[index];

                // Encontra o botão correspondente e faz a jogada
                Button buttonToClick = (Button)mainGameGrid.FindName($"btn{row}{col}");
                if (buttonToClick != null)
                {
                    MakeMove(buttonToClick, row, col);

                    // Verifica o estado do jogo após a jogada da IA
                    if (CheckForWinner())
                    {
                        statusTextBlock.Text = $"Jogador O (Computador) venceu!";
                        DisableAllButtons();
                    }
                    else if (IsBoardFull())
                    {
                        statusTextBlock.Text = "Empate!";
                        DisableAllButtons();
                    }
                    else
                    {
                        playerXTurn = !playerXTurn; // Troca o turno de volta para o jogador humano
                        statusTextBlock.Text = $"Vez do Jogador {(playerXTurn ? "X" : "O")}";
                    }
                }
            }
        }

        private bool CheckForWinner()
        {
            // Verificar linhas
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] != ' ' && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2]) return true;
            }

            // Verificar colunas
            for (int i = 0; i < 3; i++)
            {
                if (board[0, i] != ' ' && board[0, i] == board[1, i] && board[1, i] == board[2, i]) return true;
            }

            // Verificar diagonais
            if (board[0, 0] != ' ' && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2]) return true;
            if (board[0, 2] != ' ' && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0]) return true;

            return false;
        }

        private bool IsBoardFull()
        {
            return turnCount == 9;
        }

        private void DisableAllButtons()
        {
            foreach (UIElement element in mainGameGrid.Children)
            {
                if (element is Button button && button.Name.StartsWith("btn"))
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void ReiniciarJogo_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        private void VoltarAoMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowModeSelection(); // Volta para a tela de seleção de modo
            InitializeGame(); // Reseta o estado do jogo para garantir que esteja limpo
        }
    }
}